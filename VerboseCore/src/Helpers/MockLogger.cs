using System.Text;
using System.Collections.Generic;

using VerboseCore.Abstract;
using VerboseCore.Interfaces;

namespace VerboseCore.Helpers
{
    public class MockLogger : ILogger
    {
        public void EmitLog(LogType log, APosition pos, string msg, List<object> values) { }
        public void EmitWarning(WarningType warning, APosition pos, string msg, List<object> values) { }
        public void EmitError(ErrorType error, APosition pos, string msg, List<object> values) { }
        public void Emit(string? str) { }
    }

    public class TestLogger : ILogger
    {
        private StringBuilder _sb = new();
        public void EmitLog(LogType log, APosition pos, string msg, List<object> values) { }
        public void EmitWarning(WarningType warning, APosition pos, string msg, List<object> values) { }
        public void EmitError(ErrorType error, APosition pos, string msg, List<object> values) { }
        public void Emit(string? str) => _sb.Append(str);
        public string Get() => _sb.ToString();
    }
}
