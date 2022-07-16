﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using SparkCore.Analytics;
using SparkCore.Analytics.Binding;
using SparkCore.Analytics.Binding.Tree;
using SparkCore.Analytics.Symbols;
using SparkCore.Analytics.Syntax.Tree;
using SparkCore.IO.Diagnostics;
using ReflectionBindingFlags = System.Reflection.BindingFlags;

namespace SparkCore;

public class Compilation
{
    private BoundGlobalScope _globalScope;
    public Compilation(params SyntaxTree[] syntaxTrees)
        : this(null, syntaxTrees)
    {
    }
    private Compilation(Compilation previous, params SyntaxTree[] syntaxTrees)
    {
        Previous = previous;
        SyntaxTrees = syntaxTrees.ToImmutableArray();
    }

    public Compilation Previous
    {
        get;
    }
    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }
    public ImmutableArray<FunctionSymbol> Functions => GlobalScope.Functions;
    public ImmutableArray<VariableSymbol> Variables => GlobalScope.Variables;

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (_globalScope == null)
            {
                var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTrees);
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

            foreach(var builtin in builtinFunction)
            {
                if(seenSymbolNames.Add(builtin.Name))
                    yield return builtin;
            }

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
            submission = submission.Previous;
        }
    }

    public Compilation ContinueWith(SyntaxTree syntaxTree)
    {
        return new Compilation(this, syntaxTree);
    }

    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
    {
        var parseDiagnostics = SyntaxTrees.SelectMany(st => st.Diagnostics);
        var diagnostics = parseDiagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();

        if (diagnostics.Any())
        {
            return new EvaluationResult(diagnostics, null);
        }
        var program = Binder.BindProgram(GlobalScope);

        var appPath = Environment.GetCommandLineArgs()[0];
        var appDirectory = Path.GetDirectoryName(appPath);
        var cfgPath = Path.Combine(appDirectory, "cfg.dot");
        var cfgStatement = !program.Statement.Statements.Any() && program.Functions.Any()
                              ? program.Functions.Last().Value
                              : program.Statement;
        var cfg = ControlFlowGraph.Create(cfgStatement);
        using (var streamWriter = new StreamWriter(cfgPath))
            cfg.WriteTo(streamWriter);


        if (program.Diagnostics.Any())
            return new EvaluationResult(program.Diagnostics.ToImmutableArray(), null);

        var evaluator = new Evaluator(program, variables);
        var value = evaluator.Evaluate();
        return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
    }

    public void EmitTree(TextWriter writer)
    {
        var program = Binder.BindProgram(GlobalScope);
        if (program.Statement.Statements.Any())
        {
            program.Statement.WriteTo(writer);
        }
        else
        {
            foreach(var functionBody in program.Functions)
            {
                if (!GlobalScope.Functions.Contains(functionBody.Key))
                    continue;
                functionBody.Key.WriteTo(writer);
                writer.WriteLine();
                functionBody.Value.WriteTo(writer);
            }
        }
    }

    public void EmitTree(FunctionSymbol function, TextWriter writer)
    {
        var program = Binder.BindProgram(GlobalScope);
        function.WriteTo(writer);
        writer.WriteLine();
        if (!program.Functions.TryGetValue(function, out var body))
            return;
        body.WriteTo(writer);
    }
}
