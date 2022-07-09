using System.Collections.Immutable;
using SparkCore.Analytics.Binding.Tree.Statements;
using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree;

internal sealed class BoundProgram
{
    public BoundProgram(BoundGlobalScope globalScope, DiagnosticBag diagnostics, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies)
    {
        GlobalScope = globalScope;
        Diagnostics = diagnostics;
        FunctionBodies = functionBodies;
    }

    public BoundGlobalScope GlobalScope
    {
        get;
    }
    public DiagnosticBag Diagnostics
    {
        get;
    }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> FunctionBodies
    {
        get;
    }
}
