using System;
using System.IO;
using System.Collections.Generic;

using VerboseCore.Abstract;
using VerboseCore.Interfaces;
using VerboseCore.Entities;
using VerboseCLI.Entities;

namespace VerboseCLI
{
    public class Logger: ILogger
    {
        readonly Locale Locale = Locale.LocaleEN;
        public TextWriter StdOut { get; set; } = Console.Out;
        public TextWriter StdErr { get; set; } = Console.Error;

        public Logger()
        {
            Locale = Locale.LocaleEN;
        }
        public Logger(Locale locale)
        {
            Locale = locale;
        }

        public void EmitLog(LogType log, APosition pos, string msg, List<object> values) =>
            Emit(StdOut, Locale.LogToLTU(log), pos, msg, values);
        public void EmitWarning(WarningType warning, APosition pos, string msg, List<object> values) =>
            Emit(StdOut, Locale.WarningToLTU(warning), pos, msg, values);
        public void EmitError(ErrorType error, APosition pos, string msg, List<object> values)
        {
            switch (error)
            {
                case ErrorType.ExpectedToken:
                    HandleExpectedSymbol(ref values);
                    break;
                case ErrorType.ExpectedSymbol:
                    HandleExpectedSymbol(ref values);
                    break;
                case ErrorType.FunctionDefinitionReturnMismatch:
                    values = HandleFunctionDefinitionReturnMismatch(values);
                    break;
                case ErrorType.FunctionDefinitionParamTypeMismatch:
                    values = HandleFunctionDefinitionParamTypeMismatch(values);
                    break;
                default:
                    break;
            }
            Emit(StdErr, Locale.ErrorToLTU(error), pos, msg, values);
        }

        public void Emit(string? str)
        {
            StdOut.WriteLine(str);
            StdOut.Flush();
        }

        public void Emit(LTU ltu, object p1)
        {
            string str = Locale.Stringify(ltu);
            StdOut.WriteLine(string.Format(str, p1));
            StdOut.Flush();
        }

        public void Emit(TextWriter std, LTU ltu, APosition pos, string msg, List<object> values)
        {
            string str = Locale.Stringify(ltu);
            switch (values.Count)
            {
                case 1:
                    std.WriteLine(string.Format(str, pos, msg, values[0]));
                    StdOut.Flush();
                    break;
                case 2:
                    std.WriteLine(string.Format(str, pos, msg, values[0], values[1]));
                    StdOut.Flush();
                    break;
                case 3:
                    std.WriteLine(string.Format(str, pos, msg, values[0], values[1], values[2]));
                    StdOut.Flush();
                    break;
                case 4:
                    std.WriteLine(string.Format(str, pos, msg, values[0], values[1], values[2], values[3]));
                    StdOut.Flush();
                    break;
                default:
                    std.WriteLine(string.Format(str, pos, msg));
                    StdOut.Flush();
                    break;
            }
        }

        public void HandleExpectedSymbol(ref List<object> values)
        {
            // morph objects back to SymbolTypes, translate them to LTUs, stringify them with our locale, join
            values = new()
            {
                string.Join(", ",
                values.ConvertAll(st =>
                    Locale.Stringify(
                        Locale.SymbolToLTU((SymbolType)st)
                    )
                )
            )
            };
        }

        public List<object> HandleFunctionDefinitionReturnMismatch(List<object> values)
        {
            // translate VariableTypes to strings
            values[2] = Locale.Stringify(StaticTranslator.VariableTypeToLTU.Translate((VariableType)values[2]));
            values[3] = Locale.Stringify(StaticTranslator.VariableTypeToLTU.Translate((VariableType)values[3]));
            return values;
        }

        public List<object> HandleFunctionDefinitionParamTypeMismatch(List<object> values)
        {
            // keep first two objects, merge the rest by three into strings
            List<object> new_values = new();
            new_values.Add(values[0]);
            new_values.Add(values[1]);
            new_values.Add(((bool)values[2] ? Locale.Stringify(LTU.TKNKeywordMutable) : "") + " " +
                Locale.Stringify(StaticTranslator.VariableTypeToLTU.Translate((VariableType)values[3])) + " " +
                values[4]);
            new_values.Add(((bool)values[5] ? Locale.Stringify(LTU.TKNKeywordMutable) : "") + " " +
                Locale.Stringify(StaticTranslator.VariableTypeToLTU.Translate((VariableType)values[6])) + " " +
                values[7]);
            return new_values;
        }
    }
}
