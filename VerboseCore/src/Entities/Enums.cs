namespace VerboseCore.Entities
{
    public enum TokenType
    {
        Any,
        SpecialNUL,
        SpecialETX,
        SpecialEOL,

        ValueNumber,
        ValueText,
        ValueComment,
        ValueFact,
        ValueIdentifier,

        OperatorDot,                // ;
        OperatorComma,              // ,
        OperatorTernaryYes,         // ?
        OperatorTernaryNo,          // :
        OperatorParenthesisOpen,    // (
        OperatorParenthesisClose,   // )
        OperatorConcatenate,        // ++
        OperatorNoneTest,           // ??

        ArithmeticAdd,              // +
        ArithmeticSub,              // -
        ArithmeticMul,              // *
        ArithmeticDiv,              // /
        ArithmeticMod,              // %

        ComparatorEqual,            // ==
        ComparatorNotEqual,         // !=
        ComparatorEqualText,        // ===
        ComparatorNotEqualText,     // !==
        ComparatorLess,             // <
        ComparatorGreater,          // >
        ComparatorLessEqual,        // <=
        ComparatorGreaterEqual,     // >=

        KeywordAnd,                 // AND
        KeywordBegin,               // BEGIN
        KeywordCall,                // CALL
        KeywordDefault,             // DEFAULT
        KeywordDo,                  // DO
        KeywordElse,                // ELSE
        KeywordEnd,                 // END
        KeywordFact,                // FACT
        KeywordFalse,               // FALSE
        KeywordFor,                 // FOR
        KeywordFunction,            // FUNCTION
        KeywordIs,                  // IS
        KeywordIf,                  // IF
        KeywordMatch,               // MATCH
        KeywordMutable,             // MUTABLE
        KeywordNone,                // NONE
        KeywordNow,                 // NOW
        KeywordNot,                 // NOT
        KeywordNothing,             // NOTHING
        KeywordNumber,              // NUMBER
        KeywordOr,                  // OR
        KeywordOtherwise,           // OTHERWISE
        KeywordOverride,            // OVERRIDE
        KeywordPattern,             // PATTERN
        KeywordPipe,                // PIPE
        KeywordReturn,              // RETURN
        KeywordReturns,             // RETURNS
        KeywordSkip,                // SKIP
        KeywordStop,                // STOP
        KeywordText,                // TEXT
        KeywordThen,                // THEN
        KeywordTrue,                // TRUE
        KeywordValue,               // VALUE
        KeywordWhile,               // WHILE
        KeywordWith,                // WITH
    }

    public enum InstructionType
    {
        Any,
        InstructionNUL,
        ExpressionAssignment,
        ExpressionPipe,
        ExpressionTernary,
        ExpressionBinary,
        ExpressionUnary,
        ExpressionNoneTest,
        ExpressionIdentifier,
        ExpressionLiteral,
        ExpresionCall,
        DeclarationVariable,
        DeclarationFunction,
        DeclarationPattern,
        StatementCompound,
        StatementIf,
        StatementWhile,
        StatementJump,
        StatementEmpty,
    }

    public enum UnaryOperatorType
    {
        Any,
        ArithmeticNegate,
        LogicalNot,
        OperatorNoneTest,
    }

    public enum BinaryOperatorType
    {
        Any,
        ArithmeticAdd,
        ArithmeticSub,
        ArithmeticMul,
        ArithmeticDiv,
        ArithmeticMod,
        LogicalAnd,
        LogicalOr,
        ComparatorEqual,
        ComparatorNotEqual,
        ComparatorEqualText,
        ComparatorNotEqualText,
        ComparatorLess,
        ComparatorGreater,
        ComparatorLessEqual,
        ComparatorGreaterEqual,
        OperatorConcatenate,
        OperatorAssignment,
    }

    public enum VariableType
    {
        Any,
        Number,
        Text,
        Fact,
        Nothing,
    }

    public enum JumpType
    {
        Any,
        Skip,
        Stop,
        Return
    }

    public enum BuiltInType
    {
        Any,
        Print,
        Quit,
        FizzBuzz,
        First,
        Last,
        Body,
        Tail,
        Split,
        BackSplit,
    }
}
