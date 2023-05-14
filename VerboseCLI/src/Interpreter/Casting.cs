using System;

using VerboseCore.Entities;
using VerboseCore.Exceptions;
using VerboseCore.Interfaces;
using VerboseCore.Helpers;

namespace VerboseCLI.Interpreter
{
    public static class Casting
    {
        public static double? AsNumber(object? value) => value switch
        {
            double => Convert.ToDouble(value),
            string => CastDouble(Convert.ToString(value)!, out var result) ? result : null,
            bool => Convert.ToBoolean(value) ? 1 : 0,
            _ => null,
        };

        public static string? AsText(object? value) => value switch
        {
            double => Convert.ToString(value)!.Replace(',', '.'),
            string => Convert.ToString(value),
            bool => Convert.ToBoolean(value) ? "TRUE" : "FALSE",
            _ => null,
        };

        public static bool? AsFact(object? value) => value switch
        {
            double => Convert.ToDouble(value) != 0,
            string => Convert.ToString(value)!.Length != 0,
            bool => Convert.ToBoolean(value),
            _ => null,
        };

        public static object? AsType(object? value, VariableType type) => type switch
        {
            VariableType.Number => AsNumber(value),
            VariableType.Text => AsText(value),
            VariableType.Fact => AsFact(value),
            _ => null,
        };

        public static VariableType GetType(object? value) => value switch
        {
            double => VariableType.Number,
            string => VariableType.Text,
            bool => VariableType.Fact,
            _ => VariableType.Nothing,
        };

        static bool CastDouble(string str, out double result)
        {
            double sign = 1;
            if (str[0] == '-')
            {
                str = str[1..];
                sign = -1;
            }
            var lexer = Helpers.MakeStringLexer(str);
            IToken? token;
            try
            {
                token = lexer.BuildNumber();
            }
            catch (LexerError)
            {
                token = null;
            }

            result = token != null ? sign * Convert.ToDouble(token.Value) : new();
            return token != null;
        }

        static bool IsValidIdentifier(string str)
        {
            var lexer = Helpers.MakeStringLexer(str);
            IToken? token;
            try
            {
                token = lexer.BuildIdOrKeyword();
            }
            catch (LexerError)
            {
                token = null;
            }

            if (token == null) return false;
            return token.Type == TokenType.ValueIdentifier;
        }
    }
}
