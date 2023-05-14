using System.Collections.Generic;

using VerboseCore.Abstract;
using VerboseCore.Entities;

namespace VerboseCore.Interfaces
{
    public interface IDeclaration : IInstruction
    {
    }

    public interface IDeclarator
    {
        public APosition Position { get; set; }
        public bool Mutable { get; }
        public VariableType VarType { get; }
        public IExpressionIdentifier Identifier { get; }
    }

    public interface IDeclarationVariable : IDeclaration
    {
        public IDeclarator Declarator { get; }
        public IExpression? Expression { get; }
    }

    public interface IDeclarationFunction: IDeclaration
    {
        public IDeclarator Declarator { get; }
        public List<IDeclarator> Parameters { get; }
        public IStatement Body { get; }
        public bool Override { get; }
    }

    public interface IDeclarationPattern: IDeclaration
    {
        public IDeclarator Declarator { get; }
        public IDeclarator Parameter { get; }
        public IStatement Body { get; }
        public bool Override { get; }
    }
}
