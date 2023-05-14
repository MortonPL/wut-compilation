using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using VerboseCore.Abstract;
using VerboseCore.Entities;
using VerboseCore.Interfaces;
using VerboseCLI.Interpreter;

[assembly: InternalsVisibleTo("VerboseTests")]
namespace VerboseCLI.Entities
{
    [Flags]
    public enum ScopeFlags
    {
        Any = 0x0,
        Loop = 0x1,
        Call = 0x2,
        Match = 0x4,
        LastCondition = 0x8,
        ShouldReturn = 0x10,
        ExpectValue = 0x20,
    }

    public class Scope
    {
        public class VariableRecord: Tuple<IDeclarator, object?, string?, bool>
        {
            public VariableRecord(IDeclarator decl, object? val, string? refs, bool init): base(decl, val, refs, init) { }
        }

        public class FunctionOrPatternSig: Tuple<string, int>
        {
            public FunctionOrPatternSig(string name, int @params) : base(name, @params) { }
        }

        public class FunctionOrPatternRecord : Tuple<IDeclaration, bool>
        {
            public FunctionOrPatternRecord(IDeclaration decl, bool builtin) : base(decl, builtin) { }
        }

        internal readonly Dictionary<string, VariableRecord> _variables = new();
        internal readonly Dictionary<FunctionOrPatternSig, FunctionOrPatternRecord> _functionsOrPatterns = new();

        public ScopeFlags Flags { get; set; }

        public Scope()
        {
        }

        public Scope(Dictionary<string, VariableRecord> variables) => _variables = variables;
        public Scope(Dictionary<FunctionOrPatternSig, FunctionOrPatternRecord> funcOrPatts) => _functionsOrPatterns = funcOrPatts;

        /***************************** VARIABLES ****************************************/

        public bool FindVariable(string name, out APosition? pos)
        {
            bool found = _variables.TryGetValue(name, out var v);
            pos = found ? v!.Item1.Position : null;
            return found;
        }

        public bool FindVariable(string name, out string? @ref)
        {
            bool found = _variables.TryGetValue(name, out var v);
            @ref = found ? v!.Item3 : null;
            return found;
        }

        public void AddVariable(IDeclarator declarator, object? value, string? refs, bool init)
        {
            _variables.Add(declarator.Identifier.Name, new(declarator, value, refs, init));
        }

        public bool TryGetVariableValue(string variable, out object? value, out string? @ref)
        {
            bool found = _variables.TryGetValue(variable, out var v);
            value = found ? v!.Item2 : null;
            @ref = found ? v!.Item3 : null;
            if (found && !v!.Item4)
                throw new InterpreterError(ErrorType.UninitializedVariable, new() { variable, v!.Item1.Position });
            return found;
        }

        public bool TryGetVariableDeclarator(string variable, out IDeclarator? decl, out string? @ref)
        {
            bool found = _variables.TryGetValue(variable, out var v);
            decl = found ? v!.Item1 : null;
            @ref = found ? v!.Item3 : null;
            return found;
        }

        public bool TryUpdateVariable(string variable, object? value)
        {
            bool found = _variables.TryGetValue(variable, out var decl);
            if (found)
                _variables[variable] = new(decl!.Item1, value, decl.Item3, true);
            return found;
        }

        /***************************** FUNCTIONS ****************************************/

        public bool FindFunctionOrPattern(IDeclarationFunction declaration, out APosition? pos, out bool? builtIn)
        {
            FunctionOrPatternSig key = new(
                declaration.Declarator.Identifier.Name,
                declaration.Parameters.Count);

            bool found = _functionsOrPatterns.TryGetValue(key, out var f);
            pos = found ? f!.Item1.Position : null;
            builtIn = found ? f!.Item2 : null;
            return found;
        }

        public void AddFunction(IDeclarationFunction declaration, bool builtIn=false)
        {
            FunctionOrPatternSig key = new(
                declaration.Declarator.Identifier.Name,
                declaration.Parameters.Count);

            _functionsOrPatterns.Add(key, new(declaration, builtIn));
        }

        public bool TryGetFunction(string func, int paramCount, out IDeclarationFunction? decl)
        {
            bool found = _functionsOrPatterns.TryGetValue(new(func, paramCount), out var f);
            if (found && (f!.Item1.Type == InstructionType.DeclarationPattern))
                throw new InterpreterError(ErrorType.PatternRedefinitionWithFunction, new() { func, f.Item1.Position });
            decl = found ? f!.Item1 as IDeclarationFunction : null;
            return found;
        }

        public bool TryGetFunctionSafe(string func, int paramCount, out IDeclarationFunction? decl)
        {
            bool found = _functionsOrPatterns.TryGetValue(new(func, paramCount), out var f);
            decl = found ? f!.Item1 as IDeclarationFunction : null;
            return decl != null;
        }

        public void UpdateFunction(IDeclarationFunction decl)
        {
            _functionsOrPatterns[new(decl.Declarator.Identifier.Name, decl.Parameters.Count)] = new(decl, false);
        }

        /***************************** PATTERNS ****************************************/

        public void AddPattern(IDeclarationPattern declaration, bool builtIn = false)
        {
            FunctionOrPatternSig key = new(
                declaration.Declarator.Identifier.Name,
                1);

            _functionsOrPatterns.Add(key, new(declaration, builtIn));
        }

        public bool FindFunctionOrPattern(IDeclarationPattern declaration, out APosition? pos, out bool? builtIn)
        {
            FunctionOrPatternSig key = new(
                declaration.Declarator.Identifier.Name,
                1);

            bool found = _functionsOrPatterns.TryGetValue(key, out var f);
            pos = found ? f!.Item1.Position : null;
            builtIn = found ? f!.Item2 : null;
            return found;
        }

        public bool TryGetPattern(string func, int paramCount, out IDeclarationPattern? decl)
        {
            bool found = _functionsOrPatterns.TryGetValue(new(func, paramCount), out var f);
            if (found && (f!.Item1.Type == InstructionType.DeclarationFunction))
                throw new InterpreterError(ErrorType.FunctionRedefinitionWithPattern, new() { func, f.Item1.Position });
            decl = found ? f!.Item1 as IDeclarationPattern : null;
            return found;
        }

        public bool TryGetPatternSafe(string func, int paramCount, out IDeclarationPattern? decl)
        {
            bool found = _functionsOrPatterns.TryGetValue(new(func, paramCount), out var f);
            decl = found ? f!.Item1 as IDeclarationPattern : null;
            return decl != null;
        }

        public void UpdatePattern(IDeclarationPattern decl)
        {
            _functionsOrPatterns[new(decl.Declarator.Identifier.Name, 1)] = new(decl, false);
        }

        /***************************** FLAGS ****************************************/

        public ScopeFlags GetFlag(ScopeFlags flag) => Flags & flag;
        public void SetFlag(ScopeFlags flag) => Flags |= flag;
        public void UnsetFlag(ScopeFlags flags) => Flags &= ~flags;
    }
}
