using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

using System;
using System.IO;
using System.Collections.Generic;

using VerboseCore.Entities;
using VerboseCore.Helpers;
using VerboseCore.Interfaces;

using VerboseCLI.Entities;
using VerboseCLI.Interpreter;

namespace VerboseTests.Interpreter_
{
    [TestClass]
    public partial class InterpreterTests
    {
        public class MockParser: IParser
        {
            public string BufferedError { get => ""; }

            public MockParser() { }

            public IInstruction BuildProgram() => new Instruction();
        }

        public static Interpreter MakeInterpreter(bool persistScopes = false, bool mockLogger = true)
        {
            ILogger logger = mockLogger ? new MockLogger() : new TestLogger();
            Interpreter i = new(new MockParser(), logger);
            if (persistScopes)
                i.DEBUG_PERSIST_SCOPES = true;
            return i;
        }

        public static void TestInterpreterExpression<T>(Interpreter interpreter,
            T program, object? expected, Action<T> method)
        {
            method(program);
            interpreter._lastValue.Should().BeEquivalentTo(expected, o =>
                o.RespectingRuntimeTypes().AllowingInfiniteRecursion());
        }

        public static void TestInterpreterExpression<T>(Interpreter interpreter,
            List<T> programs, List<object?> expected, Action<T> method)
        {
            for (int i = 0; i < programs.Count; i++)
            {
                method(programs[i]);
                interpreter._lastValue.Should().BeEquivalentTo(expected[i], o =>
                    o.RespectingRuntimeTypes().AllowingInfiniteRecursion(), i.ToString());
            }
        }

        public static void TestInterpreterDeclaration<T>(Interpreter interpreter,
            T program, Context expected, Action<T> method)
        {
                method(program);
                interpreter.CurrentContext._scopes.Should().BeEquivalentTo(expected._scopes, o =>
                    o.RespectingRuntimeTypes().AllowingInfiniteRecursion());
        }

        public static void TestInterpreterDeclaration<T>(Interpreter interpreter,
            List<T> programs, List<Context> expected, Action<T> method, Action reloader)
        {
            for (int i = 0; i < programs.Count; i++)
            {
                reloader();
                method(programs[i]);
                interpreter.CurrentContext._scopes.Should().BeEquivalentTo(expected[i]._scopes, o =>
                    o.RespectingRuntimeTypes().AllowingInfiniteRecursion(), i.ToString());
            }
        }

        public static void TestInterpreterException<T>(T program, ErrorType expected, Action<T> method)
        {
            Action a = () => method(program);
            a.Should().Throw<InterpreterError>().Where(e => e.Error == expected);
        }

        public static void TestInterpreterException<T>(List<T> programs, List<ErrorType> expected, Action<T> method, Action reloader)
        {
            for (int i = 0; i < programs.Count; i++)
            {
                reloader();
                Action a = () => method(programs[i]);
                a.Should().Throw<InterpreterError>(i.ToString()).Where(e => e.Error == expected[i]);
            }
        }

        public static void TestInterpreterCasting<T>(List<object?> values, List<T?> expected, Func<object?, T?> method)
        {
            for (int i = 0; i < values.Count; i++)
            {
                var actual = method(values[i]);
                actual.Should().BeEquivalentTo(expected[i], o =>
                    o.RespectingRuntimeTypes().AllowingInfiniteRecursion(), i.ToString());
            }
        }

        public static void TestInterpreterCastingAs(List<object?> values, List<VariableType> types,
            List<object?> expected, Func<object?, VariableType, object?> method)
        {
            for (int i = 0; i < values.Count; i++)
            {
                var actual = method(values[i], types[i]);
                actual.Should().BeEquivalentTo(expected[i], o =>
                    o.RespectingRuntimeTypes().AllowingInfiniteRecursion(), i.ToString());
            }
        }

        public static void TestInterpreterAcceptation(string program, string expected)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(program);
            writer.Flush();
            stream.Position = 0;

            var interpreter = new Interpreter(stream, new TestLogger());
            interpreter.BuildProgram();
            interpreter.RunProgram();
            var logger = interpreter._logger as TestLogger;
            logger!.Get().Should().Be(expected);
        }

        public static void TestInterpreterStdOut<T>(Interpreter interpreter,
            T program, string expected, Action<T> method)
        {
            method(program);
            var logger = interpreter._logger as TestLogger;
            logger!.Get().Should().Be(expected);
        }

        public static void TestInterpreterStdOut<T>(Interpreter interpreter,
            List<T> programs, List<string> expected, Action<T> method)
        {
            for (int i = 0; i < programs.Count; i++)
            {
                method(programs[i]);
                var logger = interpreter._logger as TestLogger;
                logger!.Get().Should().Be(expected[i]);
            }
        }
    }
}
