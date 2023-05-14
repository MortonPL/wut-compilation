using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using VerboseCore.Abstract;
using VerboseCore.Entities;
using VerboseCore.Exceptions;
using VerboseCore.Interfaces;
using VerboseCore.Parser;
using VerboseCLI.Entities;

[assembly: InternalsVisibleTo("VerboseTests")]
namespace VerboseCLI.Interpreter
{
    public class InterpreterError : Exception
    {
        public ErrorType Error;
        public List<object> Values;

        public InterpreterError(ErrorType type) : base()
        {
            Error = type;
            Values = new();
        }
        public InterpreterError(ErrorType type, List<object> values) : base()
        {
            Error = type;
            Values = values;
        }
    }

    public class InterpreterQuit : Exception
    {
    }

    public partial class Interpreter: IVisitor
    {
        internal readonly ILogger _logger;
        private readonly IParser _parser;

        private IInstruction _program = new Instruction();
        private APosition Position
        {
            get => CurrentContext.Position;
            set => CurrentContext.Position = value;
        }
        private int _stackLevel = 0;
        internal Context _globalContext;
        internal List<Context> _callStack;
        internal Context CurrentContext { get => _callStack[^1]; }

        internal object? _lastValue = null;
        internal protected bool DEBUG_PERSIST_SCOPES = false;

        public string BufferedError { get => _parser.BufferedError; }

        public Interpreter(IParser parser, ILogger logger)
        {
            _parser = parser;
            _logger = logger;
            _globalContext = MakeGlobalContext();
            _callStack = new();
            _callStack.Add(_globalContext);
        }

        public Interpreter(Stream stream, ILogger logger)
        {
            _parser = new Parser(stream, logger);
            _logger = logger;
            _globalContext = MakeGlobalContext();
            _callStack = new();
            _callStack.Add(_globalContext);
        }

        public static Context MakeGlobalContext()
        {
            var context = new Context() { Position = new Position(1, 1) };
            context.SetFlag(ScopeFlags.Call);
            AddBuiltIns(context);
            return context;
        }

        public void Reset()
        {
            Position = new Position();
            _program = new Instruction();
            _globalContext = MakeGlobalContext();
            _callStack = new();
            _callStack.Add(_globalContext);
            _lastValue = null;
            CurrentContext.Jump = JumpType.Any;
        }

        public static void AddBuiltIns(Context context)
        {
            Scope globalScope = new();
            foreach (var func in StandardLibrary.BuiltInFunctions)
                globalScope.AddFunction(func, true);
            foreach (var pattern in StandardLibrary.BuiltInPatterns)
                globalScope.AddPattern(pattern, true);
            context.AddScope(globalScope);
        }

        public void BuildProgram()
        {
            try
            {
                _program = _parser.BuildProgram();
            }
            catch (LexerError)
            {
            }
            catch (ParserError)
            {
            }
        }

        public void StackCountBump(IInstruction i)
        {
            if (++_stackLevel >= 1600)
                throw new InterpreterError(ErrorType.StackOverflow, new() { i.Position });
        }

        public void StackCountDown() => --_stackLevel;

        public void HandleError(InterpreterError e)
        {
            _logger.EmitError(e.Error, Position, BufferedError, e.Values);
            while (_callStack.Count > 0)
            {
                var ctx = _callStack[^1];
                _callStack.RemoveAt(_callStack.Count - 1);
                _logger.EmitError(ErrorType.StackTrace, ctx.Position, BufferedError, new() { ctx.Name });
            }
        }

        public void HandleError(ErrorType type, List<object> values)
        {
            _logger.EmitError(type, Position, BufferedError, values);
            while (_callStack.Count > 0)
            {
                var ctx = _callStack[^1];
                _callStack.RemoveAt(_callStack.Count - 1);
                _logger.EmitError(ErrorType.StackTrace, ctx.Position, BufferedError, new() { ctx.Name });
            }
        }

        public void RunProgram()
        {
            try
            {
                _program.Accept(this);
            }
            catch (InterpreterQuit)
            {
            }
            catch (InterpreterError e)
            {
                HandleError(e);
            }
        }

        public void Visit(IInstruction instruction) { }

        // Expressions
        public void Visit(IExpressionAssignment assignment)
        {
            StackCountBump(assignment);
            Position = assignment.Position;
            if (assignment.To.Type != InstructionType.ExpressionIdentifier)
                throw new InterpreterError(ErrorType.AssignmentToNotVariable);

            var ident = (IExpressionIdentifier)assignment.To;

            if ((ident.Name == "PIPE") || (ident.Name == "VALUE"))
                throw new InterpreterError(ErrorType.VariableReservedAssignment);

            IDeclarator? decl = FindVariableDeclaratorViaReference(ident, out var i);
            if (decl == null)
                throw new InterpreterError(ErrorType.UndefinedVariable, new() { ident.Name });

            if (!decl.Mutable)
                throw new InterpreterError(ErrorType.VariableImmutable, new() { ident.Name });

            CurrentContext.SetFlag(ScopeFlags.ExpectValue);
            assignment.From.Accept(this);
            CurrentContext.UnsetFlag(ScopeFlags.ExpectValue);
            object? right = Casting.AsType(_lastValue, decl.VarType);

            _callStack[i].TryUpdateVariable(decl.Identifier.Name, right);
            _lastValue = right;
            StackCountDown();
        }

        public void Visit(IExpressionPipe pipeOp)
        {
            StackCountBump(pipeOp);
            CurrentContext.SetFlag(ScopeFlags.ExpectValue);
            pipeOp.First.Accept(this);
            var first = _lastValue;
            Position = pipeOp.Position;

            // create new scope here
            CurrentContext.CreateScope();
            CurrentContext.AssureVariable(
                new Declarator(Position, true, VariableType.Number, 
                    new ExpressionIdentifier(Position, "PIPE")
                ), first
            );

            if (first != null)
            {
                if (pipeOp.Then != null)
                {
                    pipeOp.Then.Accept(this);
                    first = _lastValue;
                }
                else
                {
                    _lastValue = null;
                }
            }
            if (first == null)
            {
                if (pipeOp.Otherwise != null)
                {
                    pipeOp.Otherwise.Accept(this);
                }
                else
                {
                    _lastValue = null;
                }
            }
            CurrentContext.DeleteScope();
            StackCountDown();
        }

        public void Visit(IExpressionTernary ternaryOp)
        {
            StackCountBump(ternaryOp);
            CurrentContext.SetFlag(ScopeFlags.ExpectValue);
            ternaryOp.Condition.Accept(this);
            Position = ternaryOp.Position;
            bool? condition = Casting.AsFact(_lastValue);

            if (condition == null)
                throw new InterpreterError(ErrorType.ExpectedNotNone); // TODO

            if (condition!.Value)
                ternaryOp.OnYes.Accept(this);
            else if (!condition!.Value && ternaryOp.OnNo != null)
                ternaryOp.OnNo!.Accept(this);
            else
                _lastValue = null;
            CurrentContext.UnsetFlag(ScopeFlags.ExpectValue);
            StackCountDown();
        }

        public void Visit(IExpressionBinary binaryOp)
        {
            StackCountBump(binaryOp);
            CurrentContext.SetFlag(ScopeFlags.ExpectValue);
            binaryOp.Left.Accept(this);
            var leftVal = _lastValue;
            binaryOp.Right.Accept(this);
            var rightVal = _lastValue;
            Position = binaryOp.Position;

            switch (binaryOp.Op)
            {
                case BinaryOperatorType.ArithmeticAdd:
                case BinaryOperatorType.ArithmeticSub:
                case BinaryOperatorType.ArithmeticMul:
                case BinaryOperatorType.ArithmeticDiv:
                case BinaryOperatorType.ArithmeticMod:
                {
                    _lastValue = SolveBinaryArithmetic(leftVal, rightVal, binaryOp.Op);
                    break;
                }
                case BinaryOperatorType.LogicalAnd:
                case BinaryOperatorType.LogicalOr:
                {
                    _lastValue = SolveBinaryLogical(leftVal, rightVal, binaryOp.Op);
                    break;
                }
                case BinaryOperatorType.ComparatorEqual:
                case BinaryOperatorType.ComparatorNotEqual:
                case BinaryOperatorType.ComparatorLess:
                case BinaryOperatorType.ComparatorLessEqual:
                case BinaryOperatorType.ComparatorGreater:
                case BinaryOperatorType.ComparatorGreaterEqual:
                {
                    _lastValue = SolveBinaryComparison(leftVal, rightVal, binaryOp.Op);
                    break;
                }
                case BinaryOperatorType.ComparatorEqualText:
                case BinaryOperatorType.ComparatorNotEqualText:
                {
                    _lastValue = SolveBinaryTextComparison(leftVal, rightVal, binaryOp.Op);
                    break;
                }
                case BinaryOperatorType.OperatorConcatenate:
                {
                    _lastValue = SolveBinaryConcatenation(leftVal, rightVal, binaryOp.Op);
                    break;
                }
                default:
                    break;
            }
            CurrentContext.UnsetFlag(ScopeFlags.ExpectValue);
            StackCountDown();
        }

        public void Visit(IExpressionUnary unaryOp)
        {
            StackCountBump(unaryOp);
            CurrentContext.SetFlag(ScopeFlags.ExpectValue);
            Position = unaryOp.Position;
            unaryOp.Value.Accept(this);
            var val = _lastValue;

            if (unaryOp.Op == UnaryOperatorType.ArithmeticNegate)
                val = -Casting.AsNumber(val);
            else if (unaryOp.Op == UnaryOperatorType.LogicalNot)
                val = !Casting.AsFact(val);

            _lastValue = val;
            CurrentContext.UnsetFlag(ScopeFlags.ExpectValue);
            StackCountDown();
        }

        public void Visit(IExpressionNoneTest expression)
        {
            StackCountBump(expression);
            CurrentContext.SetFlag(ScopeFlags.ExpectValue);
            Position = expression.Position;
            expression.Value.Accept(this);
            _lastValue = _lastValue == null;
            CurrentContext.UnsetFlag(ScopeFlags.ExpectValue);
            StackCountDown();
        }

        public void Visit(IExpressionCall call)
        {
            StackCountBump(call);
            Position = call.Position;
            var name = call.Identifier.Name;

            if (name == null)
                throw new InterpreterError(ErrorType.CallableNameNotText);

            if (!_globalContext.TryGetFunctionOrPattern(name, call.Args.Count, out IDeclaration? funcOrPattern) &&
                !CurrentContext.TryGetFunctionOrPattern(name, call.Args.Count, out funcOrPattern))
                throw new InterpreterError(ErrorType.UndefinedFunctionOrPattern, new() { name });

            if (funcOrPattern!.Type == InstructionType.DeclarationFunction)
                HandleFunctionCall((DeclarationFunction)funcOrPattern, call.Args, new Position(Position));
            else
                HandlePatternCall((DeclarationPattern)funcOrPattern, call.Args[0], new Position(Position));
            StackCountDown();
        }

        public void Visit(IExpressionIdentifier identifier)
        {
            StackCountBump(identifier);
            Position = identifier.Position;

            IDeclarator? decl = FindVariableDeclaratorViaReference(identifier, out var i);
            if (decl == null)
                if (identifier.Name == "PIPE")
                    throw new InterpreterError(ErrorType.PipeNotInPipeline, new() { });
                else if (identifier.Name == "VALUE")
                    throw new InterpreterError(ErrorType.ValueNotInMatch, new() { });
                else
                    throw new InterpreterError(ErrorType.UndefinedVariable, new() { identifier.Name });
            FindVariableViaReference(decl.Identifier, i);
            StackCountDown();
        }

        public void Visit(IExpressionLiteral literal)
        {
            StackCountBump(literal);
            Position = literal.Position;
            _lastValue = literal.Value;
            StackCountDown();
        }

        // Declarations
        public void Visit(IDeclarationVariable variable)
        {
            StackCountBump(variable);
            Position = variable.Position;

            if ((variable.Declarator.Identifier.Name == "PIPE") ||
                (variable.Declarator.Identifier.Name == "VALUE"))
                throw new InterpreterError(ErrorType.VariableReserved, new() { variable.Declarator.Identifier.Name });

            if (variable.Expression != null)
            {
                CurrentContext.SetFlag(ScopeFlags.ExpectValue);
                variable.Expression.Accept(this);
                _lastValue = Casting.AsType(_lastValue!, variable.Declarator.VarType);
                CurrentContext.UnsetFlag(ScopeFlags.ExpectValue);
            }
            CurrentContext.AddVariable(variable.Declarator, _lastValue, null, variable.Expression != null);
            StackCountDown();
        }

        public void Visit(IDeclarationFunction function)
        {
            StackCountBump(function);
            Position = function.Position;
            _globalContext.CheckIfBuiltIn(function);

            CurrentContext.AddFunction(function);
            StackCountDown();
        }

        public void Visit(IDeclarationPattern pattern)
        {
            StackCountBump(pattern);
            Position = pattern.Position;
            _globalContext.CheckIfBuiltIn(pattern);

            CurrentContext.AddPattern(pattern);
            StackCountDown();
        }

        // Statements
        public void Visit(IStatementCompound compound)
        {
            StackCountBump(compound);
            CurrentContext.CreateScope();
            foreach (var instruction in compound.Instructions)
            {
                switch (CurrentContext.Jump)
                {
                    case JumpType.Skip:
                    case JumpType.Stop:
                        StackCountDown();
                        return;
                    case JumpType.Return:
                        {
                            if (CurrentContext.GetFlag(ScopeFlags.Call))
                                CurrentContext.SetFlag(ScopeFlags.ShouldReturn);
                            StackCountDown();
                            return;
                        }
                    default:
                        break;
                }
                instruction.Accept(this);

                if (CurrentContext.GetFlagSum(ScopeFlags.Match | ScopeFlags.LastCondition))
                    break;
            }

            if (!DEBUG_PERSIST_SCOPES)
                CurrentContext.DeleteScope();
            StackCountDown();
        }

        public void Visit(IStatementIf @if)
        {
            StackCountBump(@if);
            CurrentContext.SetFlag(ScopeFlags.ExpectValue);
            @if.Condition.Accept(this);
            CurrentContext.UnsetFlag(ScopeFlags.ExpectValue);
            Position = @if.Position;
            var condition = Casting.AsFact(_lastValue);

            if (condition == null)
                throw new InterpreterError(ErrorType.ExpectedNotNone);
            if (condition!.Value)
                CurrentContext.SetFlag(ScopeFlags.LastCondition);
            else
                CurrentContext.UnsetFlag(ScopeFlags.LastCondition);

            if (condition!.Value)
                @if.OnTrue.Accept(this);
            else if (!condition!.Value && @if.OnFalse != null)
                @if.OnFalse.Accept(this);
            StackCountDown();
        }

        public void Visit(IStatementJump jump)
        {
            StackCountBump(jump);
            Position = jump.Position;
            CurrentContext.Jump = jump.JumpType;

            if (CurrentContext.GetFlag(ScopeFlags.Match))
                throw new InterpreterError(ErrorType.JumpInMatch);

            if ((CurrentContext.Jump == JumpType.Skip) && !CurrentContext.GetFlag(ScopeFlags.Loop))
                throw new InterpreterError(ErrorType.InvalidSkip);
            if ((CurrentContext.Jump == JumpType.Stop) && !CurrentContext.GetFlag(ScopeFlags.Loop))
                throw new InterpreterError(ErrorType.InvalidStop);

            if (jump.JumpType == JumpType.Return)
            {
                // can only return in a function
                if (!CurrentContext.GetFlag(ScopeFlags.Call))
                    throw new InterpreterError(ErrorType.InvalidReturn);
                if (CurrentContext.ReturnType == VariableType.Nothing)
                {
                    if (jump.ReturnValue != null)
                        throw new InterpreterError(ErrorType.ReturnUnexpectedValue);
                    _lastValue = null;
                }
                else
                {
                    if (jump.ReturnValue == null)
                        throw new InterpreterError(ErrorType.ReturnExpectedValue);
                    CurrentContext.SetFlag(ScopeFlags.ExpectValue);
                    jump.ReturnValue.Accept(this);
                    CurrentContext.UnsetFlag(ScopeFlags.ExpectValue);
                    _lastValue = Casting.AsType(_lastValue, CurrentContext.ReturnType);
                }
            }
            StackCountDown();
        }
        
        public void Visit(IStatementWhile @while)
        {
            StackCountBump(@while);
            bool? condition;
            void checkCondition()
            {
                CurrentContext.SetFlag(ScopeFlags.ExpectValue);
                @while.Condition.Accept(this);
                CurrentContext.UnsetFlag(ScopeFlags.ExpectValue);
                Position = @while.Position;
                condition = Casting.AsFact(_lastValue);
                if (condition == null)
                    throw new InterpreterError(ErrorType.ExpectedNotNone);
            }

            checkCondition();

            bool @break = false;
            while (condition!.Value)
            {
                CurrentContext.SetFlag(ScopeFlags.Loop);
                switch (CurrentContext.Jump)
                {
                    case JumpType.Skip:
                        CurrentContext.Jump = JumpType.Any;
                        @while.Body.Accept(this);
                        checkCondition();
                        continue;
                    case JumpType.Stop:
                        CurrentContext.Jump = JumpType.Any;
                        @break = true;
                        break;
                    case JumpType.Return:
                        @break = true;
                        break;
                    default:
                        break;
                }
                
                if (@break)
                    break;

                @while.Body.Accept(this);
                checkCondition();
            }
            if (CurrentContext.Jump != JumpType.Return)
                CurrentContext.Jump = JumpType.Any;
            CurrentContext.UnsetFlag(ScopeFlags.Loop);
            StackCountDown();
        }
        
        public void Visit(IStatementAnonMatch match)
        {
            StackCountBump(match);
            CurrentContext.SetFlag(ScopeFlags.ExpectValue);
            match.Expression.Accept(this);
            CurrentContext.UnsetFlag(ScopeFlags.ExpectValue);
            var expr = _lastValue;
            Position = match.Position;

            CurrentContext.CreateScope();
            CurrentContext._scopes[^1].AddVariable(
                new Declarator(Position, true, Casting.GetType(expr),
                    new ExpressionIdentifier(Position, "VALUE")
                ), expr, null, true
            );
            CurrentContext.SetFlag(ScopeFlags.Match);
            match.Body.Accept(this);

            CurrentContext.DeleteScope();
            StackCountDown();
        }

        /******************************* REFERENCE HELPERS ************************/

        void FindVariableViaReference(IExpressionIdentifier identifier, int contextId)
        {
            _callStack[contextId].TryGetVariableValue(identifier.Name, out _lastValue, out _);
        }

        IDeclarator? FindVariableDeclaratorViaReference(IExpressionIdentifier identifier, out int contextId)
        {
            if (CurrentContext.TryGetVariableDeclarator(identifier.Name, out var decl, out var @ref))
            {
                var i = @ref is null ? _callStack.Count - 2 : _callStack.Count - 1;
                if (@ref != null)
                {
                    if ((identifier.Name == @ref) && (CurrentContext.Name == _globalContext.Name))
                    {
                        contextId = _callStack.Count - 1;
                        return decl;
                    }
                    var gref = @ref;
                    if (_globalContext.TryGetVariableDeclarator(gref, out decl, out gref))
                    {
                        while (gref != null)
                            _globalContext.TryGetVariableDeclarator(gref, out decl, out gref);
                        contextId = 0;
                        return decl;
                    }
                }
                while ((@ref != null) && (i >= 0))
                {
                    if ((identifier.Name == @ref) && (CurrentContext.Name == _callStack[i].Name))
                    {
                        contextId = i;
                        return decl;
                    }
                    if (_callStack[i].TryGetVariableDeclarator(@ref, out decl, out var cref))
                    {
                        @ref = cref;
                        if (@ref != null)
                            continue;
                    }
                    i -= 1;
                }
                contextId = i+1;
                return decl;
            }
            if (_globalContext.TryGetVariableDeclarator(identifier.Name, out decl, out @ref))
            {
                while (@ref != null)
                    _globalContext.TryGetVariableDeclarator(@ref, out decl, out @ref);
                contextId = 0;
                return decl;
            }
            contextId = 0;
            return null;
        }

        /******************************* CALL EXPRESSION HELPERS ************************/

        void HandleFunctionCall(DeclarationFunction func, List<IExpression> callArgs, APosition position)
        {
            if (CurrentContext.GetFlag(ScopeFlags.ExpectValue) && (func.Declarator.VarType == VariableType.Nothing))
                throw new InterpreterError(ErrorType.ReturnedNothing);

            // get arg values, cast to param types
            var args = new List<object?>();
            var vars = new List<IDeclarator?>();
            var refs = new List<string?>();
            for (int i = 0; i < callArgs.Count; i++)
            {
                // handle referencing
                string? @ref = null;
                if (callArgs[i] is IExpressionIdentifier id)
                {
                    if (CurrentContext.TryFindVariable(id.Name, out @ref))
                        vars.Add(CurrentContext.GetVariableDeclarator(id.Name));
                    else if (_globalContext.TryFindVariable(id.Name, out @ref))
                        vars.Add(CurrentContext.GetVariableDeclarator(id.Name));
                    else
                        vars.Add(null);
                }

                callArgs[i].Accept(this);
                args.Add(Casting.AsType(_lastValue, func.Parameters[i].VarType));
                refs.Add(@ref);
            }

            Context callContext = new();
            callContext.Position = position;
            CurrentContext.Position = position;
            callContext.Name = func.Declarator.Identifier.Name;
            callContext.AddScope(new(BuildArgsAsVariables(func.Parameters, args, vars, refs)));
            _callStack.Add(callContext);
            callContext.SetFlag(ScopeFlags.Call);

            callContext.ReturnType = func.Declarator.VarType;

            func.Body.Accept(this);
            _callStack.RemoveAt(_callStack.Count - 1);

            if (func.Declarator.VarType == VariableType.Nothing)
                _lastValue = null;
        }

        void HandlePatternCall(DeclarationPattern pattern, IExpression callarg, APosition position)
        {
            object? arg;
            IDeclarator? var = null;
            string? @ref = null;
            // handle referencing
            if (callarg is IExpressionIdentifier id)
            {
                if (CurrentContext.TryFindVariable(id.Name, out @ref))
                    var = CurrentContext.GetVariableDeclarator(id.Name);
                else
                    var = null;
            }

            CurrentContext.SetFlag(ScopeFlags.ExpectValue);
            callarg.Accept(this);
            CurrentContext.UnsetFlag(ScopeFlags.ExpectValue);
            arg = Casting.AsType(_lastValue, pattern.Parameter.VarType);

            Context callContext = new();
            callContext.Position = position;
            callContext.Name = pattern.Declarator.Identifier.Name;
            var value = new Declarator(pattern.Parameter.Position, false, pattern.Parameter.VarType,
                new ExpressionIdentifier(pattern.Parameter.Position, "VALUE"));

            callContext.AddScope(new(BuildArgsAsVariables(new() { value, pattern.Parameter },
                new() { arg, arg }, new() { null, var }, new() { null, @ref})));
            _callStack.Add(callContext);
            callContext.SetFlag(ScopeFlags.Match);

            pattern.Body.Accept(this);
            _callStack.RemoveAt(_callStack.Count - 1);
        }

        static Dictionary<string, Scope.VariableRecord> BuildArgsAsVariables(List<IDeclarator> @params, List<object?> values,
            List<IDeclarator?> vars, List<string?> refs)
        {
            Dictionary<string, Scope.VariableRecord> dict = new();
            for (int i = 0; i < @params.Count; i++)
            {
                var @ref = (@params[i].Mutable && (vars[i] != null) && vars[i]!.Mutable) ? (refs[i] ?? vars[i]!.Identifier.Name) : null;
                dict.Add(@params[i].Identifier.Name, new(@params[i], values[i], @ref, true));
            }
            return dict;
        }

        /************************** BINARY EXPRESSION HELPERS **************************/

        static double? SolveBinaryArithmetic(object? left, object? right, BinaryOperatorType type)
        {
            double? leftNum = Casting.AsNumber(left);
            double? rightNum = Casting.AsNumber(right);
            if (leftNum == null || rightNum == null)
                return null;
            try
            {
                double? result = type switch
                {
                    BinaryOperatorType.ArithmeticAdd => checked(leftNum! + rightNum!),
                    BinaryOperatorType.ArithmeticSub => checked(leftNum! - rightNum!),
                    BinaryOperatorType.ArithmeticMul => checked(leftNum! * rightNum!),
                    BinaryOperatorType.ArithmeticDiv => checked(leftNum! / rightNum!),
                    BinaryOperatorType.ArithmeticMod => checked(leftNum! % rightNum!),
                    _ => null,
                };
                // doubles return infinity instead of raising an exception
                if (result != null && double.IsInfinity(result!.Value))
                    throw new InterpreterError(ErrorType.DivisionByZero);
                return result;
            }
            catch (OverflowException)
            {
                throw new InterpreterError(ErrorType.DynamicNumberOverflow);
            }
        }

        static bool? SolveBinaryLogical(object? left, object? right, BinaryOperatorType type)
        {
            bool? leftBool = Casting.AsFact(left);
            bool? rightBool = Casting.AsFact(right);
            if (leftBool == null || rightBool == null)
                return null;
            return type switch
            {
                BinaryOperatorType.LogicalAnd => leftBool!.Value && rightBool!.Value,
                BinaryOperatorType.LogicalOr => leftBool!.Value || rightBool!.Value,
                _ => null,
            };
        }

        static bool? SolveBinaryComparison(object? left, object? right, BinaryOperatorType type)
        {
            switch (Casting.GetType(left))
            {
                case VariableType.Number:
                {
                    double? leftNum = Casting.AsNumber(left);
                    double? rightNum = Casting.AsNumber(right);
                    if (leftNum == null || rightNum == null)
                        return null;
                    return type switch
                    {
                        BinaryOperatorType.ComparatorEqual => Math.Abs(leftNum!.Value - rightNum!.Value) <= Shared.EPSILON,
                        BinaryOperatorType.ComparatorNotEqual => Math.Abs(leftNum!.Value - rightNum!.Value) > Shared.EPSILON,
                        BinaryOperatorType.ComparatorGreater => leftNum!.Value > rightNum!.Value,
                        BinaryOperatorType.ComparatorGreaterEqual => (Math.Abs(leftNum!.Value - rightNum!.Value) <= Shared.EPSILON) || (leftNum!.Value > rightNum!.Value),
                        BinaryOperatorType.ComparatorLess => leftNum!.Value < rightNum!.Value,
                        BinaryOperatorType.ComparatorLessEqual => (Math.Abs(leftNum!.Value - rightNum!.Value) <= Shared.EPSILON) || (leftNum!.Value < rightNum!.Value),
                        _ => null,
                    };
                }
                case VariableType.Text:
                {
                    string? leftText = Casting.AsText(left);
                    string? rightText = Casting.AsText(right);
                    if (leftText == null || rightText == null)
                        return null;
                    return type switch
                    {
                        BinaryOperatorType.ComparatorEqual => leftText.Length == rightText.Length,
                        BinaryOperatorType.ComparatorNotEqual => leftText.Length != rightText.Length,
                        BinaryOperatorType.ComparatorGreater => leftText.Length > rightText.Length,
                        BinaryOperatorType.ComparatorGreaterEqual => leftText.Length >= rightText.Length,
                        BinaryOperatorType.ComparatorLess => leftText.Length < rightText.Length,
                        BinaryOperatorType.ComparatorLessEqual => leftText.Length <= rightText.Length,
                        _ => null,
                    };
                }
                case VariableType.Fact:
                {
                    bool? leftFact = Casting.AsFact(left);
                    bool? rightFact = Casting.AsFact(right);
                    if (leftFact == null || rightFact == null)
                        return null;
                    return type switch
                    {
                        BinaryOperatorType.ComparatorEqual => leftFact == rightFact,
                        BinaryOperatorType.ComparatorNotEqual => leftFact != rightFact,
                        BinaryOperatorType.ComparatorGreater => leftFact!.Value && !rightFact!.Value,
                        BinaryOperatorType.ComparatorGreaterEqual => (leftFact == rightFact) || (leftFact!.Value && !rightFact!.Value),
                        BinaryOperatorType.ComparatorLess => !leftFact!.Value && rightFact!.Value,
                        BinaryOperatorType.ComparatorLessEqual => (leftFact == rightFact) || (!leftFact!.Value && rightFact!.Value),
                        _ => null,
                    };
                }
                default:
                    return null;
            }
        }

        static bool? SolveBinaryTextComparison(object? left, object? right, BinaryOperatorType type)
        {
            string? leftText = Casting.AsText(left);
            string? rightText = Casting.AsText(right);
            if (leftText == null || rightText == null)
                return null;
            return type switch
            {
                BinaryOperatorType.ComparatorEqualText => leftText == rightText,
                BinaryOperatorType.ComparatorNotEqualText => leftText != rightText,
                _ => null,
            };
        }

        static string? SolveBinaryConcatenation(object? left, object? right, BinaryOperatorType type)
        {
            string? leftText = Casting.AsText(left);
            string? rightText = Casting.AsText(right);
            if (leftText == null || rightText == null)
                return null;
            return type switch
            {
                BinaryOperatorType.OperatorConcatenate => leftText + rightText,
                _ => null,
            };
        }
    }
}
