using System.Collections.Generic;

using VerboseCore.Abstract;
using VerboseCore.Interfaces;

namespace VerboseCore.Entities
{
    public class Declaration: Instruction, IDeclaration
    {
    }

    public class Declarator: IDeclarator
    {
        public APosition Position { get; set; }
        public bool Mutable { get; }
        public VariableType VarType { get; }
        public IExpressionIdentifier Identifier { get; }

        public Declarator(APosition position, bool mutable, VariableType varType, IExpressionIdentifier identifier)
        {
            Position = position;
            Mutable = mutable;
            VarType = varType;
            Identifier = identifier;
        }

        public Declarator(bool mutable, VariableType varType, IExpressionIdentifier identifier)
        {
            Position = new Position();
            Mutable = mutable;
            VarType = varType;
            Identifier = identifier;
        }
    }

    public class DeclarationVariable : Declaration, IDeclarationVariable
    {
        public IDeclarator Declarator { get; }
        public IExpression? Expression { get; }

        public DeclarationVariable(APosition position, IDeclarator declarator, IExpression? expression)
        {
            Position = position;
            _type = InstructionType.DeclarationVariable;
            Declarator = declarator;
            Expression = expression;
        }

        public DeclarationVariable(IDeclarator declarator, IExpression? expression)
        {
            Position = new Position();
            _type = InstructionType.DeclarationVariable;
            Declarator = declarator;
            Expression = expression;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class DeclarationFunction: Declaration, IDeclarationFunction
    {
        public IDeclarator Declarator { get; }
        public List<IDeclarator> Parameters { get; }
        public IStatement Body { get; }
        public bool Override { get; }

        public DeclarationFunction(APosition position, IDeclarator declarator, List<IDeclarator> parameters, IStatement body, bool @override)
        {
            Position = position;
            _type = InstructionType.DeclarationFunction;
            Declarator = declarator;
            Parameters = parameters;
            Body = body;
            Override = @override;
        }

        public DeclarationFunction(IDeclarator declarator, List<IDeclarator> parameters, IStatement body, bool @override)
        {
            Position = new Position();
            _type = InstructionType.DeclarationFunction;
            Declarator = declarator;
            Parameters = parameters;
            Body = body;
            Override = @override;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public class DeclarationPattern : Declaration, IDeclarationPattern
    {
        public IDeclarator Declarator { get; }
        public IDeclarator Parameter { get; }
        public IStatement Body { get; }
        public bool Override { get; }

        public DeclarationPattern(APosition position, IDeclarator declarator, IDeclarator parameter, IStatement body, bool @override)
        {
            Position = position;
            _type = InstructionType.DeclarationPattern;
            Declarator = declarator;
            Parameter = parameter;
            Body = body;
            Override = @override;
        }

        public DeclarationPattern(IDeclarator declarator, IDeclarator parameter, IStatement body, bool @override)
        {
            Position = new Position();
            _type = InstructionType.DeclarationPattern;
            Declarator = declarator;
            Parameter = parameter;
            Body = body;
            Override = @override;
        }

        public new void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
