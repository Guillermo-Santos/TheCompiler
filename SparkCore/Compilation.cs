using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using SparkCore.Analytics;
using SparkCore.Analytics.Binding;
using SparkCore.Analytics.Binding.Tree;
using SparkCore.Analytics.Emit;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.IO.Diagnostics;
using ReflectionBindingFlags = System.Reflection.BindingFlags;

namespace SparkCore;

public class Compilation
{
    private BoundGlobalScope _globalScope;
    private Compilation(bool isScript, Compilation previous, params SyntaxTree[] syntaxTrees)
    {
        IsScript = isScript;
        Previous = previous;
        SyntaxTrees = syntaxTrees.ToImmutableArray();
    }

    public static Compilation Create(params SyntaxTree[] syntaxTrees)
    {
        return new Compilation(isScript: false, null, syntaxTrees);
    }
    public static Compilation CreateScript(Compilation previous, params SyntaxTree[] syntaxTrees)
    {
        return new Compilation(isScript: true, previous, syntaxTrees);
    }

    public bool IsScript
    {
        get;
    }
    public Compilation Previous
    {
        get;
    }
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
    public FunctionSymbol MainFunctions => GlobalScope.MainFunction;
    public ImmutableArray<FunctionSymbol> Functions => GlobalScope.Functions;
    public ImmutableArray<VariableSymbol> Variables => GlobalScope.Variables;

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (_globalScope == null)
            {
                var globalScope = Binder.BindGlobalScope(IsScript, Previous?.GlobalScope, SyntaxTrees);
                Interlocked.CompareExchange(ref _globalScope, globalScope, null);
            }
            return _globalScope;
        }
    }

    public IEnumerable<Symbol> GetSymbols()
    {
        var submission = this;
        var seenSymbolNames = new HashSet<string>();
        while (submission != null)
        {
            const ReflectionBindingFlags bindingFlags = ReflectionBindingFlags.Static |
                                                        ReflectionBindingFlags.Public |
                                                        ReflectionBindingFlags.NonPublic;

            var builtinFunction = typeof(BuiltinFunctions)
                                    .GetFields(bindingFlags)
                                    .Where(bf => bf.FieldType == typeof(FunctionSymbol))
                                    .Select(bf => (FunctionSymbol)bf.GetValue(obj: null))
                                    .ToList();

            foreach (var function in submission.Functions)
            {
                if(seenSymbolNames.Add(function.Name))
                    yield return function;
            }

            foreach (var variable in submission.Variables)
            {
                if(seenSymbolNames.Add(variable.Name))
                    yield return variable;
            }

            foreach (var builtin in builtinFunction)
            {
                if (seenSymbolNames.Add(builtin.Name))
                    yield return builtin;
            }
            submission = submission.Previous;
        }
    }

    public Compilation ContinueWith(SyntaxTree syntaxTree)
    {
        return CreateScript(this, syntaxTree);
    }

    private BoundProgram GetProgram()
    {
        var previous = Previous == null ? null : Previous.GetProgram();
        return Binder.BindProgram(IsScript, previous, GlobalScope);
    }
    // TODO: Create function to expose diagnostics of the binder, without the need of the 'Evaluate' or 'Emit' funcions.
    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
    {
        var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);
        var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();

        if (diagnostics.Any())
        {
            return new EvaluationResult(diagnostics, null);
        }
        var program = GetProgram();

        // TODO: Sacar la impresion a una funcion. Crear directorio \Temp y logica de limpiado con cada cierre de la app.
        // Control Flow evaluation
        //foreach (var function in program.Functions)
        //{
        //    var appPath = Environment.GetCommandLineArgs()[0];
        //    var appDirectory = Path.GetDirectoryName(appPath);
        //    var cfgPath = Path.Combine(appDirectory, $"{function.Key.Name}.dot");
        //    var cfgStatement = function.Value;
        //    var cfg = ControlFlowGraph.Create(cfgStatement);
        //    using (var streamWriter = new StreamWriter(cfgPath))
        //        cfg.WriteTo(streamWriter);
        //}
        // =========================

        if (program.Diagnostics.Any())
            return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);

        var evaluator = new Evaluator(program, variables);
        var value = evaluator.Evaluate();
        return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
    }

    public void EmitTree(TextWriter writer)
    {
        if (GlobalScope.MainFunction != null)
        {
            EmitTree(GlobalScope.MainFunction, writer);
        }
        else if (GlobalScope.ScriptFunction != null)
        {
            EmitTree(GlobalScope.ScriptFunction, writer);
        }

        foreach (var function in GlobalScope.Functions.Where(f => f != GlobalScope.MainFunction && f != GlobalScope.ScriptFunction))
        {
            EmitTree(function, writer);
        }
    }

    public void EmitTree(FunctionSymbol function, TextWriter writer)
    {
        var program = GetProgram();
        function.WriteTo(writer);
        writer.WriteLine();
        if (!program.Functions.TryGetValue(function, out var body))
            return;
        body.WriteTo(writer);
    }
    public ImmutableArray<Diagnostic> Emit(string moduleName, string[] references, string outputPath)
    {
        var program = GetProgram();
        return Emitter.Emit(program, moduleName, references, outputPath);
    }
}