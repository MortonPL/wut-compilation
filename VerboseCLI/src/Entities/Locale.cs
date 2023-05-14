using VerboseCore.Interfaces;

namespace VerboseCLI.Entities
{
    public class Locale
    {
        public readonly string Name;
        private readonly Translator<string> Translator;

        public Locale()
        {
            Name = "";
            Translator = new("");
        }

        public Locale(string name, Translator<string> translator)
        {
            Name = name;
            Translator = translator;
        }

        public static readonly Locale LocaleEN = new("en-us", new Translator<string>("Unknown LTU!!!")
        {
            {LTU.UnknownEntry,                      "Unknown locale entry!!!"},
            {LTU.LOGInitialized,                    "{2} Initializing Verbose CLI Interpreter..."},
            {LTU.ERRNoSource,                       "[VE00] : No source code provided!" },
            {LTU.ERRNumOverflow,                    "[VE01] @ {0}: Constant number overflow!"},
            {LTU.ERRNumNotDecimal,                  "[VE02] @ {0}: Incorrect decimal part '{1}'!"},
            {LTU.ERRNumNotInBase,                   "[VE03] @ {0}: Incorrect non-base 10 number '{1}'!"},
            {LTU.ERRNumUnknownBase,                 "[VE03] @ {0}: Unknown radix '{1}'!"},
            {LTU.ERRTextTooBig,                     "[VE04] @ {0}: Text constant size is above the limit of {2}!"},
            {LTU.ERRTextSuddenEnd,                  "[VE05] @ {0}: Code ended with unclosed text constant!"},
            {LTU.ERRCommentTooBig,                  "[VE06] @ {0}: Comment size is above the limit of {2}!"},
            {LTU.ERRCommentSuddenEnd,               "[VE07] @ {0}: Code ended with unclosed comment!"},
            {LTU.ERRIdentifierTooBig,               "[VE08] @ {0}: Identifier name exceeded size limit of {2}!"},
            {LTU.ERRUnknownEscape,                  "[VE09] @ {0}: Unrecognised escape sequence '{2}' at '{1}'!" },
            {LTU.ERRUnknownToken,                   "[VE10] @ {0}: Unknown token '{1}'!" },
            {LTU.ERRExpectedToken,                  "[VE10] @ {0}: Expected one of the following tokens: '{2}' but got '{1}'!"},
            {LTU.ERRExpectedSymbol,                 "[VE11] @ {0}: Expected one of the following symbols: '{2}' but got '{1}'!"},
            {LTU.ERRDynamicNumberOverflow,          "[VE12] @ {0}: Number overflow!" },
            {LTU.ERRDivisionByZero,                 "[VE13] @ {0}: Division by zero!" },
            {LTU.ERRExpectedNotNone,                "[VE14] @ {0}: Expected not none!" },
            {LTU.ERRAssignmentToNotVariable,        "[VE14] @ {0}: The left-hand side of an assigment must be a variable!" },
            {LTU.ERRInvalidSkip,                    "[VE15] @ {0}: SKIP can only be used inside of a while or for loop!" },
            {LTU.ERRInvalidStop,                    "[VE16] @ {0}: STOP can only be used inside of a while or for loop!" },
            {LTU.ERRInvalidReturn,                  "[VE17] @ {0}: RETURN can be used with a value only inside a function body!" },
            {LTU.ERRVariableRedefinition,           "[VE18] @ {0}: Redefinition of variable '{2}' defined at {3}!" },
            {LTU.ERRFunctionRedeclaration,          "[VE19] @ {0}: Redeclaration of function '{2}' declared at {3}!" },
            {LTU.ERRFunctionRedefinition,           "[VE20] @ {0}: Redefinition of function '{2}' defined at {3}!" },
            {LTU.ERRFunctionDefinitionReturnMismatch,"[VE21] @ {0}: Definition of function '{2}' declared at {3} doesn't match declared return type of '{5}' (got '{4}')!" },
            {LTU.ERRFunctionDefinitionParamTypeMismatch,"[VE22] @ {0}: Definition of function '{2}' declared at {3} doesn't match one of the declared parameters '{5}' (got '{4}')!" },
            {LTU.ERRBuiltinRedefinition,            "[VE23] @ {0}: Redefinition of built-in function or pattern '{2}!'" },
            {LTU.ERRPatternRedeclaration,           "[VE24] @ {0}: Redeclaration of pattern '{2}' declared at {3}!" },
            {LTU.ERRPatternRedefinition,            "[VE25] @ {0}: Redefinition of pattern '{2}' defined at {3}!" },
            {LTU.ERRUndefinedVariable,              "[VE26] @ {0}: Undefined variable '{2}'!" },
            {LTU.ERRPatternRedefinitionWithFunction, "[VE27] @ {0}: Definition of function '{2}' collides with declared pattern at {3}!" },
            {LTU.ERRVariableReserved,               "[VE28] @ {0}: Variable '{2}' is reserved and can't be defined by the user!" },
            {LTU.ERRPipeNotInPipeline,              "[VE29] @ {0}: PIPE variable can be only used inside a pipeline expression!" },
            {LTU.ERRValueNotInMatch,                "[VE30] @ {0}: VALUE variable can be only used inside an anonymous match or a pattern!" },
            {LTU.ERRBadFunctionOverride,            "[VE31] @ {0}: Cannot override function '{2}' that has not been declared!" },
            {LTU.ERRBadPatternOverride,             "[VE32] @ {0}: Cannot override pattern '{2}' that has not been declared!" },
            {LTU.ERRNotImplementedFunction,         "[VE33] @ {0}: Attempting to call an unimplemented function '{2}' declared at {3}!" },
            {LTU.ERRNotImplementedPattern,          "[VE34] @ {0}: Attempting to call an unimplemented pattern '{2}' declared at {3}!" },
            {LTU.ERRCallableNameNotText,            "[VE35] @ {0}: Name of function/pattern to call did not evaluate to text!" },
            {LTU.ERRReturnExpectedValue,            "[VE36] @ {0}: Expected to return a value from function!" },
            {LTU.ERRReturnUnexpectedValue,          "[VE37] @ {0}: Functions that return nothing cannot return a value!" },
            {LTU.ERRVariableReservedAssignment,     "[VE38] @ {0}: Cannot assign to reserved PIPE or VALUE variables!" },
            {LTU.ERRVariableImmutable,              "[VE39] @ {0}: Cannot assign to immutable variable '{2}'!" },
            {LTU.ERRStackTrace,                     "    Occured @ {0} inside of '{2}'..." },
            {LTU.ERRFunctionRedefinitionWithPattern, "[VE40] @ {0}: Definition of pattern '{2}' collides with declared function at {3}!" },
            {LTU.ERRJumpInMatch,                    "[VE41] @ {0}: Jump instructions inside an anonymous match or pattern are illegal!" },
            {LTU.ERRPatternDefinitionParamTypeMismatch, "[VE42] @ {0}: Definition of pattern '{2}' declared at {3} doesn't match the declared parameter '{5}' (got '{4}')!"},
            {LTU.ERRReturnedNothing,                "[VE43] @ {0}: Expected a value, but function or pattern returns type NOTHING!" },
            {LTU.ERRDuplicateParam,                 "[VE44] @ {0}: Duplicate parameter name '{2}' in function '{3}'!" },
            {LTU.ERRUndefinedFunctionOrPattern,     "[VE45] @ {0}: Undefined function or pattern '{2}'!" },
            {LTU.ERRStackOverflow,                  "[VE46] @ {0}: Stack overflow!" },
            {LTU.ERRUninitializedVariable,          "[VE47] @ {0}: Trying to evaluate uninitialized variable '{2}' defined at {3}!" },
            {LTU.TKNValueNumber, "number" },
            {LTU.TKNValueText, "text" },
            {LTU.TKNValueComment, "comment" },
            {LTU.TKNValueFact, "fact" },
            {LTU.TKNValueIdentifier, "identifier" },
            {LTU.TKNOperatorDot, ";" },
            {LTU.TKNOperatorComma, "," },
            {LTU.TKNOperatorTernaryYes, "?" },
            {LTU.TKNOperatorTernaryNo, ":" },
            {LTU.TKNOperatorParenthesisOpen, "(" },
            {LTU.TKNOperatorParenthesisClose, ")" },
            {LTU.TKNOperatorConcatenate, "++" },
            {LTU.TKNOperatorNoneTest, "??" },
            {LTU.TKNArithmeticAdd, "+" },
            {LTU.TKNArithmeticSub, "-" },
            {LTU.TKNArithmeticMul, "*" },
            {LTU.TKNArithmeticDiv, "/" },
            {LTU.TKNArithmeticMod, "%" },
            {LTU.TKNComparatorEqual, "==" },
            {LTU.TKNComparatorNotEqual, "!=" },
            {LTU.TKNComparatorEqualText, "===" },
            {LTU.TKNComparatorNotEqualText, "!==" },
            {LTU.TKNComparatorLess, "<" },
            {LTU.TKNComparatorGreater, ">" },
            {LTU.TKNComparatorLessEqual, "<=" },
            {LTU.TKNComparatorGreaterEqual, ">=" },
            {LTU.TKNKeywordAnd, "AND" },
            {LTU.TKNKeywordBegin, "BEGIN" },
            {LTU.TKNKeywordCall, "CALL" },
            {LTU.TKNKeywordDefault, "DEFAULT" },
            {LTU.TKNKeywordDo, "DO" },
            {LTU.TKNKeywordElse, "ELSE" },
            {LTU.TKNKeywordEnd, "END" },
            {LTU.TKNKeywordFact, "FACT" },
            {LTU.TKNKeywordFalse, "FALSE" },
            {LTU.TKNKeywordFor, "FOR" },
            {LTU.TKNKeywordFunction, "FUNCTION" },
            {LTU.TKNKeywordIs, "IS" },
            {LTU.TKNKeywordIf, "IF" },
            {LTU.TKNKeywordMatch, "MATCH" },
            {LTU.TKNKeywordMutable, "MUTABLE" },
            {LTU.TKNKeywordNone, "NONE" },
            {LTU.TKNKeywordNow, "NOW" },
            {LTU.TKNKeywordNot, "NOT" },
            {LTU.TKNKeywordNothing, "NOTHING" },
            {LTU.TKNKeywordNumber, "NUMBER" },
            {LTU.TKNKeywordOr, "OR" },
            {LTU.TKNKeywordOtherwise, "OTHERWISE" },
            {LTU.TKNKeywordPattern, "PATTERN" },
            {LTU.TKNKeywordPipe, "PIPE" },
            {LTU.TKNKeywordReturn, "RETURN" },
            {LTU.TKNKeywordReturns, "RETURNS" },
            {LTU.TKNKeywordSkip, "SKIP" },
            {LTU.TKNKeywordStop, "STOP" },
            {LTU.TKNKeywordText, "TEXT" },
            {LTU.TKNKeywordThen, "THEN" },
            {LTU.TKNKeywordTrue, "TRUE" },
            {LTU.TKNKeywordValue, "VALUE" },
            {LTU.TKNKeywordWhile, "WHILE" },
            {LTU.TKNKeywordWith, "WITH" },
            {LTU.SYMExpression, "expression" },
            {LTU.SYMExpressionIdentifier, "identifier" },
            {LTU.SYMStatement, "statement" },
            {LTU.SYMStatementWhile, "while statement" },
            {LTU.SYMDeclarator, "declarator" },
            {LTU.SYMDeclarationVariable, "variable declaration" },
            {LTU.SYMMatchBlock, "match block" },
            {LTU.SYMReturnType, "return type" },
            {LTU.VARAny, "" },
            {LTU.VARNumber, "NUMBER" },
            {LTU.VARText, "TEXT" },
            {LTU.VARFact, "FACT" },
            {LTU.VARNothing, "NOTHING" },
        });

        public string Stringify(LogType log) => Translator.Translate(LogToLTU(log));
        public string Stringify(WarningType warning) => Translator.Translate(WarningToLTU(warning));
        public string Stringify(ErrorType error) => Translator.Translate(ErrorToLTU(error));
        public string Stringify(SymbolType symbol) => Translator.Translate(SymbolToLTU(symbol));
        public string Stringify(LTU ltu) => Translator.Translate(ltu);

        public static LTU LogToLTU(LogType log) => StaticTranslator.LogToLTU.Translate(log);
        public static LTU WarningToLTU(WarningType warning) => StaticTranslator.WarningToLTU.Translate(warning);
        public static LTU ErrorToLTU(ErrorType error) => StaticTranslator.ErrorToLTU.Translate(error);
        public static LTU SymbolToLTU(SymbolType symbol) => StaticTranslator.SymbolToLTU.Translate(symbol);
    }
}
