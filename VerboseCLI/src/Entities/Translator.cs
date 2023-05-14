using System.Collections.Generic;

using VerboseCore.Interfaces;
using VerboseCore.Entities;

namespace VerboseCLI.Entities
{
    public enum LTU
    {
        UnknownEntry,
        LOGInitialized,
        ERRNoSource,
        // Lexer
        ERRNumOverflow,
        ERRNumNotDecimal,
        ERRNumNotInBase,
        ERRNumUnknownBase,
        ERRTextTooBig,
        ERRTextSuddenEnd,
        ERRCommentTooBig,
        ERRCommentSuddenEnd,
        ERRIdentifierTooBig,
        ERRUnknownEscape,
        ERRUnknownToken,
        // Parser
        ERRExpectedToken,
        ERRExpectedSymbol,
        ERRDuplicateParam,
        // TokenTypes
        TKNValueNumber,
        TKNValueText,
        TKNValueComment,
        TKNValueFact,
        TKNValueIdentifier,
        TKNOperatorDot,
        TKNOperatorComma,
        TKNOperatorTernaryYes,
        TKNOperatorTernaryNo,
        TKNOperatorParenthesisOpen,
        TKNOperatorParenthesisClose,
        TKNOperatorConcatenate,
        TKNOperatorNoneTest,
        TKNArithmeticAdd,
        TKNArithmeticSub,
        TKNArithmeticMul,
        TKNArithmeticDiv,
        TKNArithmeticMod,
        TKNComparatorEqual,
        TKNComparatorNotEqual,
        TKNComparatorEqualText,
        TKNComparatorNotEqualText,
        TKNComparatorLess,
        TKNComparatorGreater,
        TKNComparatorLessEqual,
        TKNComparatorGreaterEqual,
        TKNKeywordAnd,
        TKNKeywordBegin,
        TKNKeywordCall,
        TKNKeywordDefault,
        TKNKeywordDo,
        TKNKeywordElse,
        TKNKeywordEnd,
        TKNKeywordFact,
        TKNKeywordFalse,
        TKNKeywordFor,
        TKNKeywordFunction,
        TKNKeywordIs,
        TKNKeywordIf,
        TKNKeywordMatch,
        TKNKeywordMutable,
        TKNKeywordNone,
        TKNKeywordNow,
        TKNKeywordNot,
        TKNKeywordNothing,
        TKNKeywordNumber,
        TKNKeywordOr,
        TKNKeywordOtherwise,
        TKNKeywordOverride,
        TKNKeywordPattern,
        TKNKeywordPipe,
        TKNKeywordReturn,
        TKNKeywordReturns,
        TKNKeywordSkip,
        TKNKeywordStop,
        TKNKeywordText,
        TKNKeywordThen,
        TKNKeywordTrue,
        TKNKeywordValue,
        TKNKeywordWhile,
        TKNKeywordWith,
        // Symbols
        SYMExpression,
        SYMExpressionIdentifier,
        SYMStatement,
        SYMStatementWhile,
        SYMDeclarator,
        SYMDeclarationVariable,
        SYMMatchBlock,
        SYMReturnType,
        // Variable Types
        VARAny,
        VARNumber,
        VARText,
        VARFact,
        VARNothing,
        // Interpreter
        ERRDynamicNumberOverflow,
        ERRDivisionByZero,
        ERRAssignmentToNotVariable,
        ERRExpectedNotNone,
        ERRInvalidSkip,
        ERRInvalidStop,
        ERRInvalidReturn,
        ERRVariableRedefinition,
        ERRFunctionRedeclaration,
        ERRFunctionRedefinition,
        ERRBuiltinRedefinition,
        ERRFunctionDefinitionReturnMismatch,
        ERRFunctionDefinitionParamTypeMismatch,
        ERRPatternRedeclaration,
        ERRPatternRedefinition,
        ERRUndefinedVariable,
        ERRUninitializedVariable,
        ERRUndefinedFunctionOrPattern,
        ERRPatternRedefinitionWithFunction,
        ERRVariableReserved,
        ERRPipeNotInPipeline,
        ERRValueNotInMatch,
        ERRBadFunctionOverride,
        ERRBadPatternOverride,
        ERRNotImplementedFunction,
        ERRNotImplementedPattern,
        ERRCallableNameNotText,
        ERRReturnExpectedValue,
        ERRReturnUnexpectedValue,
        ERRVariableReservedAssignment,
        ERRVariableImmutable,
        ERRStackTrace,
        ERRFunctionRedefinitionWithPattern,
        ERRJumpInMatch,
        ERRPatternDefinitionParamTypeMismatch,
        ERRReturnedNothing,
        ERRStackOverflow,
    }

    public class Translator<TKey, TValue> : Dictionary<TKey, TValue> where TKey: notnull
    {
        private readonly TValue Unknown;

        public Translator(TValue unknown) : base()
        {
            Unknown = unknown;
        }

        public Translator(TValue unknown, Dictionary<TKey, TValue> dict) : base(dict)
        {
            Unknown = unknown;
        }

        public TValue Translate(TKey key) => TryGetValue(key, out TValue? value) ? value : Unknown;
    }

    public class Translator<TValue> : Translator<LTU, TValue>
    {
        public Translator(TValue unknown) : base(unknown)
        {
        }

        public Translator(TValue unknown, Translator<LTU, TValue> dict): base(unknown, dict)
        {
        }
    }

    internal static class StaticTranslator
    {
        public static Translator<LogType, LTU> LogToLTU = new(LTU.UnknownEntry)
        {
            {LogType.DummyLog, LTU.UnknownEntry},
            {LogType.Initialized, LTU.LOGInitialized },
        };
        public static Translator<WarningType, LTU> WarningToLTU = new(LTU.UnknownEntry)
        {
            {WarningType.DummyWarning, LTU.UnknownEntry}
        };
        public static Translator<ErrorType, LTU> ErrorToLTU = new(LTU.UnknownEntry)
        {
            { ErrorType.NoSource, LTU.ERRNoSource },
            { ErrorType.NumberOverflow, LTU.ERRNumOverflow },
            { ErrorType.NumberNotADecimal, LTU.ERRNumNotDecimal },
            { ErrorType.NumberNotInBase, LTU.ERRNumNotInBase },
            { ErrorType.NumberUnknownBase, LTU.ERRNumUnknownBase },
            { ErrorType.TextTooBig, LTU.ERRTextTooBig },
            { ErrorType.TextSuddenETX, LTU.ERRTextSuddenEnd },
            { ErrorType.CommentTooBig, LTU.ERRCommentTooBig },
            { ErrorType.CommentSuddenETX, LTU.ERRCommentSuddenEnd },
            { ErrorType.IdentifierTooBig, LTU.ERRIdentifierTooBig },
            { ErrorType.UnknownEscape, LTU.ERRUnknownEscape },
            { ErrorType.UnknownToken, LTU.ERRUnknownToken },
            { ErrorType.DuplicateParam , LTU.ERRDuplicateParam },
            { ErrorType.ExpectedToken, LTU.ERRExpectedToken },
            { ErrorType.ExpectedSymbol, LTU.ERRExpectedSymbol },
            { ErrorType.DynamicNumberOverflow, LTU.ERRDynamicNumberOverflow },
            { ErrorType.DivisionByZero, LTU.ERRDivisionByZero },
            { ErrorType.AssignmentToNotVariable, LTU.ERRAssignmentToNotVariable },
            { ErrorType.ExpectedNotNone, LTU.ERRExpectedNotNone },
            { ErrorType.InvalidSkip, LTU.ERRInvalidSkip },
            { ErrorType.InvalidStop, LTU.ERRInvalidStop },
            { ErrorType.InvalidReturn, LTU.ERRInvalidReturn },
            { ErrorType.VariableRedefinition, LTU.ERRVariableRedefinition },
            { ErrorType.FunctionRedeclaration, LTU.ERRFunctionRedeclaration },
            { ErrorType.FunctionRedefinition, LTU.ERRFunctionRedefinition },
            { ErrorType.BuiltinRedefinition, LTU.ERRBuiltinRedefinition },
            { ErrorType.FunctionDefinitionReturnMismatch, LTU.ERRFunctionDefinitionReturnMismatch },
            { ErrorType.FunctionDefinitionParamTypeMismatch, LTU.ERRFunctionDefinitionParamTypeMismatch },
            { ErrorType.PatternRedeclaration, LTU.ERRPatternRedeclaration },
            { ErrorType.PatternRedefinition, LTU.ERRPatternRedefinition },
            { ErrorType.UndefinedVariable, LTU.ERRUndefinedVariable },
            { ErrorType.UndefinedFunctionOrPattern, LTU.ERRUndefinedFunctionOrPattern },
            { ErrorType.PatternRedefinitionWithFunction, LTU.ERRPatternRedefinitionWithFunction },
            { ErrorType.VariableReserved, LTU.ERRVariableReserved},
            { ErrorType.PipeNotInPipeline, LTU.ERRPipeNotInPipeline },
            { ErrorType.ValueNotInMatch, LTU.ERRValueNotInMatch },
            { ErrorType.BadFunctionOverride, LTU.ERRBadFunctionOverride },
            { ErrorType.BadPatternOverride, LTU.ERRBadPatternOverride },
            { ErrorType.NotImplementedFunction , LTU.ERRNotImplementedFunction },
            { ErrorType.NotImplementedPattern, LTU.ERRNotImplementedPattern },
            { ErrorType.CallableNameNotText , LTU.ERRCallableNameNotText },
            { ErrorType.ReturnExpectedValue , LTU.ERRReturnExpectedValue },
            { ErrorType.ReturnUnexpectedValue, LTU.ERRReturnUnexpectedValue },
            { ErrorType.VariableReservedAssignment , LTU.ERRVariableReservedAssignment },
            { ErrorType.VariableImmutable , LTU.ERRVariableImmutable },
            { ErrorType.StackTrace , LTU.ERRStackTrace },
            { ErrorType.FunctionRedefinitionWithPattern , LTU.ERRFunctionRedefinitionWithPattern },
            { ErrorType.JumpInMatch, LTU.ERRJumpInMatch },
            { ErrorType.PatternDefinitionParamTypeMismatch , LTU.ERRPatternDefinitionParamTypeMismatch },
            { ErrorType.ReturnedNothing , LTU.ERRReturnedNothing },
            { ErrorType.StackOverflow , LTU.ERRStackOverflow },
            { ErrorType.UninitializedVariable, LTU.ERRUninitializedVariable },
        };

        public static Translator<SymbolType, LTU> SymbolToLTU = new(LTU.UnknownEntry)
        {
            { SymbolType.ValueNumber,              LTU.TKNValueNumber},
            { SymbolType.ValueText,                LTU.TKNValueText},
            { SymbolType.ValueComment,             LTU.TKNValueComment},
            { SymbolType.ValueFact,                LTU.TKNValueFact},
            { SymbolType.ValueIdentifier,          LTU.TKNValueIdentifier},
            { SymbolType.OperatorDot,              LTU.TKNOperatorDot},
            { SymbolType.OperatorComma,            LTU.TKNOperatorComma},
            { SymbolType.OperatorTernaryYes,       LTU.TKNOperatorTernaryYes},
            { SymbolType.OperatorTernaryNo,        LTU.TKNOperatorTernaryNo},
            { SymbolType.OperatorParenthesisOpen,  LTU.TKNOperatorParenthesisOpen},  
            { SymbolType.OperatorParenthesisClose, LTU.TKNOperatorParenthesisClose }, 
            { SymbolType.OperatorConcatenate,      LTU.TKNOperatorConcatenate},
            { SymbolType.OperatorNoneTest,         LTU.TKNOperatorNoneTest},
            { SymbolType.ArithmeticAdd,            LTU.TKNArithmeticAdd},
            { SymbolType.ArithmeticSub,            LTU.TKNArithmeticSub},
            { SymbolType.ArithmeticMul,            LTU.TKNArithmeticMul},
            { SymbolType.ArithmeticDiv,            LTU.TKNArithmeticDiv},
            { SymbolType.ArithmeticMod,            LTU.TKNArithmeticMod},
            { SymbolType.ComparatorEqual,          LTU.TKNComparatorEqual},
            { SymbolType.ComparatorNotEqual,       LTU.TKNComparatorNotEqual},
            { SymbolType.ComparatorEqualText,      LTU.TKNComparatorEqualText},
            { SymbolType.ComparatorNotEqualText,   LTU.TKNComparatorNotEqualText},
            { SymbolType.ComparatorLess,           LTU.TKNComparatorLess},
            { SymbolType.ComparatorGreater,        LTU.TKNComparatorGreater},
            { SymbolType.ComparatorLessEqual,      LTU.TKNComparatorLessEqual},
            { SymbolType.ComparatorGreaterEqual,   LTU.TKNComparatorGreaterEqual},
            { SymbolType.KeywordAnd,               LTU.TKNKeywordAnd},
            { SymbolType.KeywordBegin,             LTU.TKNKeywordBegin},
            { SymbolType.KeywordCall,              LTU.TKNKeywordCall},
            { SymbolType.KeywordDefault,           LTU.TKNKeywordDefault},
            { SymbolType.KeywordDo,                LTU.TKNKeywordDo},
            { SymbolType.KeywordElse,              LTU.TKNKeywordElse},
            { SymbolType.KeywordEnd,               LTU.TKNKeywordEnd},
            { SymbolType.KeywordFact,              LTU.TKNKeywordFact},
            { SymbolType.KeywordFalse,             LTU.TKNKeywordFalse},
            { SymbolType.KeywordFor,               LTU.TKNKeywordFor},
            { SymbolType.KeywordFunction,          LTU.TKNKeywordFunction},
            { SymbolType.KeywordIs,                LTU.TKNKeywordIs},
            { SymbolType.KeywordIf,                LTU.TKNKeywordIf},
            { SymbolType.KeywordMatch,             LTU.TKNKeywordMatch},
            { SymbolType.KeywordMutable,           LTU.TKNKeywordMutable},
            { SymbolType.KeywordNone,              LTU.TKNKeywordNone},
            { SymbolType.KeywordNow,               LTU.TKNKeywordNow},
            { SymbolType.KeywordNot,               LTU.TKNKeywordNot},
            { SymbolType.KeywordNothing,           LTU.TKNKeywordNothing},
            { SymbolType.KeywordNumber,            LTU.TKNKeywordNumber},
            { SymbolType.KeywordOr,                LTU.TKNKeywordOr},
            { SymbolType.KeywordOtherwise,         LTU.TKNKeywordOtherwise},
            { SymbolType.KeywordOverride,          LTU.TKNKeywordOverride},
            { SymbolType.KeywordPattern,           LTU.TKNKeywordPattern},
            { SymbolType.KeywordPipe,              LTU.TKNKeywordPipe},
            { SymbolType.KeywordReturn,            LTU.TKNKeywordReturn},
            { SymbolType.KeywordReturns,           LTU.TKNKeywordReturns},
            { SymbolType.KeywordSkip,              LTU.TKNKeywordSkip},
            { SymbolType.KeywordStop,              LTU.TKNKeywordStop},
            { SymbolType.KeywordText,              LTU.TKNKeywordText},
            { SymbolType.KeywordThen,              LTU.TKNKeywordThen},
            { SymbolType.KeywordTrue,              LTU.TKNKeywordTrue},
            { SymbolType.KeywordValue,             LTU.TKNKeywordValue},
            { SymbolType.KeywordWhile,             LTU.TKNKeywordWhile},
            { SymbolType.KeywordWith,              LTU.TKNKeywordWith},
            { SymbolType.Expression,               LTU.SYMExpression},
            { SymbolType.ExpressionIdentifier,     LTU.SYMExpressionIdentifier},
            { SymbolType.Statement,                LTU.SYMStatement},
            { SymbolType.StatementWhile,           LTU.SYMStatementWhile},
            { SymbolType.Declarator,               LTU.SYMDeclarator},
            { SymbolType.DeclarationVariable,      LTU.SYMDeclarationVariable},
            { SymbolType.MatchBlock,               LTU.SYMMatchBlock},
            { SymbolType.ReturnType,               LTU.SYMReturnType},
        };

        public static Translator<VariableType, LTU> VariableTypeToLTU = new(LTU.UnknownEntry)
        {
            { VariableType.Any,     LTU.VARAny },
            { VariableType.Number,  LTU.VARNumber },
            { VariableType.Text,    LTU.VARText },
            { VariableType.Fact,    LTU.VARFact },
            { VariableType.Nothing, LTU.VARNothing },
        };
    }
}
