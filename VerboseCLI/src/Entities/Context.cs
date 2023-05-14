using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using VerboseCore.Abstract;
using VerboseCore.Entities;
using VerboseCore.Exceptions;
using VerboseCore.Interfaces;
using VerboseCore.Parser;
using VerboseCLI.Interpreter;

[assembly: InternalsVisibleTo("VerboseTests")]
namespace VerboseCLI.Entities
{
    public class Context
    {
        internal List<Scope> _scopes = new();
        public VariableType ReturnType { get; set; } = VariableType.Nothing;
        public APosition Position { get; set; } = new Position();
        public string Name { get; set; } = "Main Program";
        public JumpType Jump { get; set; } = JumpType.Any;

        public Context() => _scopes = new() { new() };

        public Context(List<Scope> scopes) => _scopes = scopes;

        public void CreateScope()
        {
            _scopes.Add(new());
        }

        public void DeleteScope()
        {
            _scopes.RemoveAt(_scopes.Count - 1);
        }

        public void AddScope(Scope scope)
        {
            _scopes.Add(scope);
        }

        /***************************** VARIABLES ****************************************/

        public void AddVariable(IDeclarator declarator, object? value, string? @ref, bool init)
        {
            foreach (var scope in _scopes)
            {
                if (scope.FindVariable(declarator.Identifier.Name, out APosition? pos))
                    throw new InterpreterError(ErrorType.VariableRedefinition,
                        new() { declarator.Identifier.Name, pos! });
            }
            _scopes.Last().AddVariable(declarator, value, @ref, init);
        }

        public void AssureVariable(IDeclarator declarator, object? value, string? @ref=null)
        {
            foreach (var scope in _scopes)
            {
                if (scope.FindVariable(declarator.Identifier.Name, out APosition _))
                    return;
            }
            _scopes.Last().AddVariable(declarator, value, @ref, true);
        }

        public bool TryFindVariable(string variable, out string? @ref)
        {
            @ref = null;
            foreach (var scope in _scopes)
            {
                if (scope.FindVariable(variable, out @ref))
                    return true;
            }
            if (variable == "PIPE")
                throw new InterpreterError(ErrorType.PipeNotInPipeline);
            if (variable == "VALUE")
                throw new InterpreterError(ErrorType.ValueNotInMatch);
            return false;
        }

        public bool TryGetVariableValue(string variable, out object? value, out string? @ref)
        {
            value = null;
            @ref = null;
            for(var i = _scopes.Count - 1; i >= 0; i--)
            {
                if (_scopes[i].TryGetVariableValue(variable, out value, out @ref))
                    return true;
            }
            if (variable == "PIPE")
                throw new InterpreterError(ErrorType.PipeNotInPipeline);
            if (variable == "VALUE")
                throw new InterpreterError(ErrorType.ValueNotInMatch);
            return false;
        }

        public bool TryGetVariableDeclarator(string variable, out IDeclarator? decl, out string? @ref)
        {
            decl = null;
            @ref = null;
            foreach (var scope in _scopes)
            {
                if (scope.TryGetVariableDeclarator(variable, out decl, out @ref))
                    return true;
            }
            return false;
        }

        public IDeclarator GetVariableDeclarator(string variable)
        {
            foreach (var scope in _scopes)
            {
                if (scope.TryGetVariableDeclarator(variable, out IDeclarator? decl, out _))
                    return decl!;
            }
            throw new InterpreterError(ErrorType.UndefinedVariable, new() { variable });
        }

        public void TryUpdateVariable(string variable, object? value)
        {
            foreach (var scope in _scopes)
            {
                if (scope.TryUpdateVariable(variable, value))
                    return;
            }
        }

        /***************************** FUNCTIONS ****************************************/

        public void AddFunction(IDeclarationFunction declaration)
        {
            var overriden = false;
            foreach (var scope in _scopes)
            {
                if (scope.FindFunctionOrPattern(declaration, out var pos, out var builtIn))
                {
                    overriden = AddFunctionHandleCollision(scope, declaration, pos!, builtIn!.Value);
                }
            }
            // can't override a function that's not declared yet!
            if (declaration.Override && !overriden)
                throw new InterpreterError(ErrorType.BadFunctionOverride, new() { declaration.Declarator.Identifier.Name });

            _scopes.Last().AddFunction(declaration);
        }

        static bool AddFunctionHandleCollision(Scope scope, IDeclarationFunction declaration, APosition pos, bool builtIn)
        {
            // if we collided with a builtin function, guaranteed error
            if (builtIn)
            {
                throw new InterpreterError(ErrorType.BuiltinRedefinition,
                    new() { declaration.Declarator.Identifier.Name });
            }
            // extract the old function, maybe it's only declared without body
            else if (scope.TryGetFunction(declaration.Declarator.Identifier.Name, declaration.Parameters.Count, out var old))
            {
                // check if absolutely everything matches - param types and return type too
                if (declaration.Declarator.VarType != old!.Declarator.VarType)
                    throw new InterpreterError(ErrorType.FunctionDefinitionReturnMismatch,
                         new() { declaration.Declarator.Identifier.Name, pos, declaration.Declarator.VarType, old.Declarator.VarType });

                // if the old one has no body, try define it
                if (old!.Body.Type == InstructionType.StatementEmpty)
                {
                    // error if the new one is without body too
                    if (declaration.Body.Type == InstructionType.StatementEmpty)
                        throw new InterpreterError(ErrorType.FunctionRedeclaration,
                            new() { declaration.Declarator.Identifier.Name, pos });

                    ValidateFunctionEqualParameters(declaration.Parameters, old.Parameters, declaration.Declarator.Identifier.Name, pos);

                    // handle override
                    if (declaration.Override)
                        scope.UpdateFunction(declaration);
                    return true;
                }
                // already defined, error
                else
                {
                    throw new InterpreterError(ErrorType.FunctionRedefinition,
                        new() { declaration.Declarator.Identifier.Name, pos! });
                }
            }
            return false;
        }

        static void ValidateFunctionEqualParameters(List<IDeclarator> one, List<IDeclarator> other, string name, APosition pos)
        {
            for (int i = 0; i < one.Count; i++)
            {
                var oneParam = one[i];
                var otherParam = other[i];

                if ((oneParam.Mutable != otherParam.Mutable) ||
                    (oneParam.VarType != otherParam.VarType) ||
                    (oneParam.Identifier.Name != otherParam.Identifier.Name))
                    throw new InterpreterError(ErrorType.FunctionDefinitionParamTypeMismatch,
                        new() { name, pos, oneParam.Mutable, oneParam.VarType, oneParam.Identifier.Name,
                            otherParam.Mutable, otherParam.VarType, otherParam.Identifier.Name });
            }
        }

        public bool TryGetFunctionOrPattern(string name, int paramCount, out IDeclaration? funcOrPattern)
        {
            funcOrPattern = null;
            for (int i = _scopes.Count - 1; i >= 0; i--)
            {
                var scope = _scopes[i];
                if (scope.TryGetFunctionSafe(name, paramCount, out IDeclarationFunction? decl))
                {
                    if (decl!.Body.Type == InstructionType.StatementEmpty)
                        throw new InterpreterError(ErrorType.NotImplementedFunction, new() { name, decl.Position });
                    funcOrPattern = decl;
                    return true;
                }

                if (scope.TryGetPatternSafe(name, 1, out IDeclarationPattern? decl2))
                {
                    if (decl2!.Body.Type == InstructionType.StatementEmpty)
                        throw new InterpreterError(ErrorType.NotImplementedPattern, new() { name, decl2.Position });
                    funcOrPattern = decl2;
                    return true;
                }
            }
            return false;
        }

        public void CheckIfBuiltIn(IDeclarationFunction function)
        {
            foreach (var scope in _scopes)
            {
                if(scope.FindFunctionOrPattern(function, out var pos, out var builtIn))
                    if (builtIn!.Value)
                        throw new InterpreterError(ErrorType.BuiltinRedefinition,
                            new() { function.Declarator.Identifier.Name });
            }
        }

        /***************************** PATTERNS ****************************************/

        public void AddPattern(IDeclarationPattern declaration)
        {
            var overriden = false;
            foreach (var scope in _scopes)
            {
                if (scope.FindFunctionOrPattern(declaration, out var pos, out var builtIn))
                {
                    overriden = AddPatternHandleCollision(scope, declaration, pos!, builtIn!.Value);
                }
            }
            // can't override a pattern that's not declared yet!
            if (declaration.Override && !overriden)
                throw new InterpreterError(ErrorType.BadPatternOverride, new() { declaration.Declarator.Identifier.Name });

            _scopes.Last().AddPattern(declaration);
        }

        static bool AddPatternHandleCollision(Scope scope, IDeclarationPattern declaration, APosition pos, bool builtIn)
        {
            // if we collided with a builtin pattern, guaranteed error
            if (builtIn)
            {
                throw new InterpreterError(ErrorType.BuiltinRedefinition,
                    new() { declaration.Declarator.Identifier.Name });
            }
            // extract the old pattern, maybe it's only declared without body
            else if (scope.TryGetPattern(declaration.Declarator.Identifier.Name, 1, out var old))
            {
                // if the old one has no body, try define it
                if (old!.Body.Type == InstructionType.StatementEmpty)
                {
                    // error if the new one is without body too
                    if (declaration.Body.Type == InstructionType.StatementEmpty)
                        throw new InterpreterError(ErrorType.PatternRedeclaration,
                            new() { declaration.Declarator.Identifier.Name, pos });

                    ValidatePatternEqualParameters(declaration.Parameter, old.Parameter, declaration.Declarator.Identifier.Name, pos);

                    // handle override
                    if (declaration.Override)
                        scope.UpdatePattern(declaration);
                    return true;
                }
                // already defined, error
                else
                {
                    throw new InterpreterError(ErrorType.PatternRedefinition,
                        new() { declaration.Declarator.Identifier.Name, pos! });
                }
            }
            return true;
        }

        static void ValidatePatternEqualParameters(IDeclarator one, IDeclarator other, string name, APosition pos)
        {
            var oneParam = one;
            var otherParam = other;

            if ((oneParam.Mutable != otherParam.Mutable) ||
                (oneParam.VarType != otherParam.VarType) ||
                (oneParam.Identifier.Name != otherParam.Identifier.Name))
                throw new InterpreterError(ErrorType.PatternDefinitionParamTypeMismatch,
                    new() { name, pos, oneParam.Mutable, oneParam.VarType, oneParam.Identifier.Name,
                        otherParam.Mutable, otherParam.VarType, otherParam.Identifier.Name });
        }

        public void CheckIfBuiltIn(IDeclarationPattern pattern)
        {
            IDeclarationFunction function = new DeclarationFunction(pattern.Declarator, new() { pattern.Parameter }, pattern.Body, pattern.Override);
            foreach (var scope in _scopes)
            {
                if (scope.FindFunctionOrPattern(function, out var pos, out var builtIn))
                    if (builtIn!.Value)
                        throw new InterpreterError(ErrorType.BuiltinRedefinition,
                            new() { pattern.Declarator.Identifier.Name });
            }
        }

        /***************************** FLAGS ****************************************/

        public bool GetFlag(ScopeFlags flag)
        {
            foreach (var scope in _scopes)
            {
                if (scope.GetFlag(flag) == flag)
                    return true;
            }
            return false;
        }

        public bool GetFlagSum(ScopeFlags flag)
        {
            ScopeFlags res =ScopeFlags.Any;
            foreach (var scope in _scopes)
            {
                res |= scope.GetFlag(flag);
                if (res == flag)
                    return true;
            }
            return false;
        }

        public void SetFlag(ScopeFlags flag) => _scopes.Last().SetFlag(flag);

        public void UnsetFlag(ScopeFlags flag) => _scopes.Last().UnsetFlag(flag);
    }
}
