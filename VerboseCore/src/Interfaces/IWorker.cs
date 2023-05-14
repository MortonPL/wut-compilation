namespace VerboseCore.Interfaces
{
    public interface IWorker<T>
    {
        public string BufferedError { get; }
    }

    public interface ILexer: IWorker<IToken>
    {
        public IToken BuildToken();
        public IToken? BuildNumber();
        public IToken? BuildIdOrKeyword();
    }

    public interface IParser : IWorker<IInstruction>
    {
        public IInstruction BuildProgram();
    }

    public interface IVisitor
    {
        public void Visit(IInstruction instruction);
        public void Visit(IBuiltInCompound instruction);
        // Expressions
        public void Visit(IExpressionAssignment instruction);
        public void Visit(IExpressionPipe instruction);
        public void Visit(IExpressionTernary instruction);
        public void Visit(IExpressionBinary instruction);
        public void Visit(IExpressionUnary instruction);
        public void Visit(IExpressionNoneTest instruction);
        public void Visit(IExpressionCall instruction);
        public void Visit(IExpressionIdentifier instruction);
        public void Visit(IExpressionLiteral instruction);

        // Declarations
        public void Visit(IDeclarationVariable instruction);
        public void Visit(IDeclarationFunction instruction);
        public void Visit(IDeclarationPattern instruction);

        // Statements
        public void Visit(IStatementCompound instruction);
        public void Visit(IStatementIf instruction);
        public void Visit(IStatementJump instruction);
        public void Visit(IStatementWhile instruction);
        public void Visit(IStatementAnonMatch instruction);
    }
}
