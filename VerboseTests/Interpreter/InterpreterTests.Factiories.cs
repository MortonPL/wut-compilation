using System;
using System.Collections.Generic;

using VerboseCore.Interfaces;
using static VerboseTests.Parser_.ParserTests;

using VerboseCLI.Entities;
using VerboseCLI.Interpreter;

using VT = VerboseCore.Entities.VariableType;

namespace VerboseTests.Interpreter_
{
    public partial class InterpreterTests
    {
        public static Context MakeContext(Scope scope) => new(new() { scope });
        public static Context MakeContext(List<Scope> scopes) => new(scopes);
        public static Context MakeGlobalContextAndAdd(Scope scope)
        {
            var c = Interpreter.MakeGlobalContext();
            c.AddScope(scope);
            return c;
        }
        public static Context MakeGlobalContextAndUnpack(Scope scope)
        {
            var c = Interpreter.MakeGlobalContext();
            foreach (var variable in scope._variables)
            {
                c.AddVariable(variable.Value.Item1, variable.Value.Item2, variable.Value.Item3, variable.Value.Item2 is null);
            }
            return c;
        }


        public static Scope MakeVariableScope(bool mutable, string name, VT type, object? value, string? @ref=null, bool init=true)
        {
            // make a scope (a dictionary of variables))
            return new(new Dictionary<string, Scope.VariableRecord>()
            {
                { name, new(Declarator(mutable, type, IdentifierE(name)), value, @ref, init) }
            });
        }

        public static Scope MakeVariableScope(List<Tuple<bool, string, VT, object?, string?, bool>> variables)
        {
            var dict = new Dictionary<string, Scope.VariableRecord>();
            foreach (var tuple in variables)
            {
                dict.Add(
                    tuple.Item2, 
                    new(
                        Declarator(tuple.Item1, tuple.Item3, IdentifierE(tuple.Item2)),
                        tuple.Item4,
                        tuple.Item5,
                        tuple.Item6
                    )
                );
            }
            return new(dict);
        }

        public static Scope MakeFunctionScope(IDeclarationFunction declaration, bool isBuiltIn=false)
        {
            // make a scope (a dictionary of functions/patterns))
            return new(new Dictionary<Scope.FunctionOrPatternSig, Scope.FunctionOrPatternRecord>()
            {
                {new(declaration.Declarator.Identifier.Name, declaration.Parameters.Count), new(declaration, isBuiltIn)}
            });
        }

        public static Scope MakeFunctionScope(List<Tuple<IDeclarationFunction, bool>> functions)
        {
            var dict = new Dictionary<Scope.FunctionOrPatternSig, Scope.FunctionOrPatternRecord>();
            foreach (var tuple in functions)
            {
                dict.Add(
                    new(tuple.Item1.Declarator.Identifier.Name, tuple.Item1.Parameters.Count),
                    new(tuple.Item1, tuple.Item2)
                );
            }
            return new(dict);
        }

        public static Scope MakePatternScope(IDeclarationPattern declaration, bool isBuiltIn=false)
        {
            // make a scope (a dictionary of functions/patterns))
            return new(new Dictionary<Scope.FunctionOrPatternSig, Scope.FunctionOrPatternRecord>()
            {
                {new(declaration.Declarator.Identifier.Name, 1), new(declaration, isBuiltIn)}
            });
        }

        public static Scope MakePatternScope(List<Tuple<IDeclarationPattern, bool>> patterns)
        {
            var dict = new Dictionary<Scope.FunctionOrPatternSig, Scope.FunctionOrPatternRecord>();
            foreach (var tuple in patterns)
            {
                dict.Add(
                    new(tuple.Item1.Declarator.Identifier.Name, 1),
                    new(tuple.Item1, tuple.Item2)
                );
            }
            return new(dict);
        }
    }
}
