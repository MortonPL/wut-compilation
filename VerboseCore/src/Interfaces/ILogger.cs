using System.Collections.Generic;

using VerboseCore.Abstract;

namespace VerboseCore.Interfaces
{
    public enum LogType
    {
        DummyLog,
        Initialized,
    }

    public enum WarningType
    {
        DummyWarning,
    }

    public enum ErrorType
    {
        Any,
        NoSource,
        // Lexer
        UnknownToken,
        NumberOverflow,
        NumberNotADecimal,
        NumberNotInBase,
        NumberUnknownBase,
        TextTooBig,
        TextSuddenETX,
        CommentTooBig,
        CommentSuddenETX,
        IdentifierTooBig,
        UnknownEscape,
        // Parser
        ExpectedToken,
        ExpectedSymbol,
        DuplicateParam,
        // Interpreter
        DynamicNumberOverflow,
        DivisionByZero,
        AssignmentToNotVariable,
        ExpectedNotNone,
        InvalidSkip,
        InvalidStop,
        InvalidReturn,
        VariableRedefinition,
        FunctionRedeclaration,
        FunctionRedefinition,
        PatternRedeclaration,
        PatternRedefinition,
        BuiltinRedefinition,
        FunctionDefinitionReturnMismatch,
        FunctionDefinitionParamTypeMismatch,
        UndefinedVariable,
        UninitializedVariable,
        UndefinedFunctionOrPattern,
        PatternRedefinitionWithFunction,
        VariableReserved,
        PipeNotInPipeline,
        ValueNotInMatch,
        BadFunctionOverride,
        BadPatternOverride,
        NotImplementedFunction,
        NotImplementedPattern,
        CallableNameNotText,
        ReturnExpectedValue,
        ReturnUnexpectedValue,
        VariableReservedAssignment,
        VariableImmutable,
        StackTrace,
        FunctionRedefinitionWithPattern,
        JumpInMatch,
        PatternDefinitionParamTypeMismatch,
        ReturnedNothing,
        StackOverflow,
    }

    public enum SymbolType
    {
        Any,
        // TokenTypes
        ValueNumber,
        ValueText,
        ValueComment,
        ValueFact,
        ValueIdentifier,
        OperatorDot,
        OperatorComma,
        OperatorTernaryYes,
        OperatorTernaryNo,
        OperatorParenthesisOpen,
        OperatorParenthesisClose,
        OperatorConcatenate,
        OperatorNoneTest,
        ArithmeticAdd,
        ArithmeticSub,
        ArithmeticMul,
        ArithmeticDiv,
        ArithmeticMod,
        ComparatorEqual,
        ComparatorNotEqual,
        ComparatorEqualText,
        ComparatorNotEqualText,
        ComparatorLess,
        ComparatorGreater,
        ComparatorLessEqual,
        ComparatorGreaterEqual,
        KeywordAnd,
        KeywordBegin,
        KeywordCall,
        KeywordDefault,
        KeywordDo,
        KeywordElse,
        KeywordEnd,
        KeywordFact,
        KeywordFalse,
        KeywordFor,
        KeywordFunction,
        KeywordIs,
        KeywordIf,
        KeywordMatch,
        KeywordMutable,
        KeywordNone,
        KeywordNow,
        KeywordNot,
        KeywordNothing,
        KeywordNumber,
        KeywordOr,
        KeywordOtherwise,
        KeywordOverride,
        KeywordPattern,
        KeywordPipe,
        KeywordReturn,
        KeywordReturns,
        KeywordSkip,
        KeywordStop,
        KeywordText,
        KeywordThen,
        KeywordTrue,
        KeywordValue,
        KeywordWhile,
        KeywordWith,
        // Symbols
        Expression,
        ExpressionIdentifier,
        Statement,
        StatementWhile,
        Declarator,
        DeclarationVariable,
        MatchBlock,
        ReturnType,
    }

    public interface ILogger
    {
        void EmitLog(LogType log, APosition pos, string msg, List<object> values);
        void EmitWarning(WarningType warning, APosition pos, string msg, List<object> values);
        void EmitError(ErrorType error, APosition pos, string msg, List<object> values);
        void Emit(string? str);
    }
}
