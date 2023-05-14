using System.Linq;
using System.Collections.Generic;

using VerboseCore.Abstract;
using VerboseCore.Entities;
using VerboseCore.Interfaces;

using VT = VerboseCore.Entities.VariableType;

namespace VerboseCLI.Interpreter
{
    public class BuiltInCompound: IBuiltInCompound
    {
        public InstructionType Type { get; set; } = InstructionType.StatementCompound;
        public APosition Position { get; set; } = new Position();
        public List<IInstruction> Instructions { get; } = new();
        public void Accept(IVisitor visitor) => visitor.Visit(this);

        public BuiltInType FunType { get; }
        public BuiltInCompound(BuiltInType type) => FunType = type;
    }

    static class StandardLibrary
    {
        static IDeclarationFunction MakeBuiltIn(string name, VT returns,
            BuiltInType builtInType, List<IDeclarator> @params) =>
                new DeclarationFunction(new Declarator(false, returns, new ExpressionIdentifier(name)),
                    @params, new BuiltInCompound(builtInType), false);

        static IDeclarator MakeDeclarator(string name, VT type, bool mutable=false) =>
            new Declarator(mutable, type, new ExpressionIdentifier(name));

        public static List<IDeclarationFunction> BuiltInFunctions = new()
        {
            // FUNCTION Print WITH TEXT t RETURNS NOTHING
            MakeBuiltIn("Print", VT.Nothing, BuiltInType.Print, new() { MakeDeclarator("t", VT.Text) }),
            // FUNCTION Quit RETURNS NOTHING
            MakeBuiltIn("Quit", VT.Nothing, BuiltInType.Quit, new()),
            // FUNCTION First WITH TEXT t RETURNS TEXT
            MakeBuiltIn("First", VT.Text, BuiltInType.First, new() { MakeDeclarator("t", VT.Text)}),
            // FUNCTION Last WITH TEXT t RETURNS TEXT
            MakeBuiltIn("Last", VT.Text, BuiltInType.Last, new() { MakeDeclarator("t", VT.Text) }),
            // FUNCTION Body WITH TEXT t RETURNS TEXT
            MakeBuiltIn("Body", VT.Text, BuiltInType.Body, new() { MakeDeclarator("t", VT.Text) }),
            // FUNCTION Tail WITH TEXT t RETURNS TEXT
            MakeBuiltIn("Tail", VT.Text, BuiltInType.Tail, new() { MakeDeclarator("t", VT.Text) }),
            // FUNCTION Split WITH TEXT source, MUTABLE TEXT head, MUTABLE TEXT tail RETURNS TEXT
            MakeBuiltIn("Split", VT.Text, BuiltInType.Split, new()
                { MakeDeclarator("source", VT.Text), MakeDeclarator("head", VT.Text, true), MakeDeclarator("tail", VT.Text, true) }),
            // FUNCTION BackSplit WITH TEXT source, MUTABLE TEXT body, MUTABLE TEXT tip RETURNS TEXT
            MakeBuiltIn("BackSplit", VT.Text, BuiltInType.BackSplit, new()
            { MakeDeclarator("source", VT.Text), MakeDeclarator("body", VT.Text, true), MakeDeclarator("tip", VT.Text, true) }),
        };

        public static List<DeclarationPattern> BuiltInPatterns = new()
        {
            // PATTERN FizzBuzz WITH NUMBER n
            new(new Declarator(false, VariableType.Nothing, new ExpressionIdentifier("FizzBuzz")),
                new Declarator(false, VariableType.Number, new ExpressionIdentifier("n")),
                new BuiltInCompound(BuiltInType.FizzBuzz), false),
        };
    }

    public partial class Interpreter
    {
        public void Visit(IBuiltInCompound builtin)
        {
            switch (builtin.FunType)
            {
                case BuiltInType.Print:
                    BuiltInPrint();
                    break;
                case BuiltInType.FizzBuzz:
                    BuiltInFizzBuzz();
                    break;
                case BuiltInType.Quit:
                    BuiltInQuit();
                    break;
                case BuiltInType.First:
                    BuiltInFirst();
                    break;
                case BuiltInType.Last:
                    BuiltInLast();
                    break;
                case BuiltInType.Body:
                    BuiltInBody();
                    break;
                case BuiltInType.Tail:
                    BuiltInTail();
                    break;
                case BuiltInType.Split:
                    BuiltInSplit();
                    break;
                case BuiltInType.BackSplit:
                    BuiltInBackSplit();
                    break;
                default:
                    break;
            }
        }

        void BuiltInPrint()
        {
            CurrentContext.TryGetVariableValue("t", out var v, out _);
            string? t = Casting.AsText(v);
            _logger.Emit(t);
        }

        void BuiltInQuit()
        {
            throw new InterpreterQuit();
        }

        void BuiltInFizzBuzz()
        {
            CurrentContext.TryGetVariableValue("n", out var v, out _);
            double? n = Casting.AsNumber(v);
            if (n == null) return;

            string str = "";

            if ((n! % 3) == 0)
                str += "Fizz";
            if ((n! % 5) == 0)
                str += "Buzz";
            if (str == "")
                str = n!.Value.ToString();
            _logger.Emit(str);
        }

        void BuiltInFirst()
        {
            CurrentContext.TryGetVariableValue("t", out var v, out _);
            string? t = Casting.AsText(v);
            _lastValue = t?.Split(' ')[0];
        }

        void BuiltInLast()
        {
            CurrentContext.TryGetVariableValue("t", out var v, out _);
            string? t = Casting.AsText(v);
            _lastValue = t?.Split(' ')[^1];
        }

        void BuiltInBody()
        {
            CurrentContext.TryGetVariableValue("t", out var v, out _);
            string? t = Casting.AsText(v);
            if (t is not null)
            {
                var list = t.Split(' ');
                _lastValue = list.Length > 1 ? string.Join(' ', list.SkipLast(1)) : null;
            }
            else
                _lastValue = null;
        }

        void BuiltInTail()
        {
            CurrentContext.TryGetVariableValue("t", out var v, out _);
            string? t = Casting.AsText(v);
            if (t is not null)
            {
                var list = t.Split(' ');
                _lastValue = list.Length > 1 ? string.Join(" ", list.Skip(1)) : null;
            }
            else
                _lastValue = null;
        }

        void BuiltInSplit()
        {
            CurrentContext.TryGetVariableValue("source", out var v, out _);
            var head = FindVariableDeclaratorViaReference(new ExpressionIdentifier("head"), out _);
            if (head is null)
                throw new InterpreterError(ErrorType.UndefinedVariable, new() { "head" });
            var tail = FindVariableDeclaratorViaReference(new ExpressionIdentifier("tail"), out _);
            if (tail is null)
                throw new InterpreterError(ErrorType.UndefinedVariable, new() { "tail" });

            string? source = Casting.AsText(v);
            string? head_ = null;
            string? tail_ = null;
            if (source is not null)
            {
                var list = source.Split(' ');
                head_ = list.Length >= 1 ? list[0] : null;
                tail_ = list.Length >= 2 ? string.Join(" ",list.Skip(1)) : null;
            }
            var headAssign = new ExpressionAssignment(new ExpressionLiteral(head_, VT.Text), new ExpressionIdentifier("head"));
            var tailAssign = new ExpressionAssignment(new ExpressionLiteral(tail_, VT.Text), new ExpressionIdentifier("tail"));
            headAssign.Accept(this);
            tailAssign.Accept(this);
        }

        void BuiltInBackSplit()
        {
            CurrentContext.TryGetVariableValue("source", out var v, out _);
            var body = FindVariableDeclaratorViaReference(new ExpressionIdentifier("body"), out _);
            if (body is null)
                throw new InterpreterError(ErrorType.UndefinedVariable, new() { "body" });
            var tip = FindVariableDeclaratorViaReference(new ExpressionIdentifier("tip"), out _);
            if (tip is null)
                throw new InterpreterError(ErrorType.UndefinedVariable, new() { "tip" });

            string? source = Casting.AsText(v);
            string? body_ = null;
            string? tip_ = null;
            if (source is not null)
            {
                var list = source.Split(' ');
                body_ = list.Length >= 1 ? string.Join(" ", list.SkipLast(1)) : null;
                tip_ = list.Length >= 2 ? list[^1] : null;
            }
            var headAssign = new ExpressionAssignment(new ExpressionLiteral(body_, VT.Text), new ExpressionIdentifier("body"));
            var tailAssign = new ExpressionAssignment(new ExpressionLiteral(tip_, VT.Text), new ExpressionIdentifier("tip"));
            headAssign.Accept(this);
            tailAssign.Accept(this);
        }
    }
}
