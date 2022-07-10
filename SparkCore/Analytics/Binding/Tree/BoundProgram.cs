using System.Collections.Immutable;
using SparkCore.Analytics.Binding.Tree.Statements;
using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree;

internal sealed class BoundProgram
{
    public BoundProgram(ImmutableArray<Diagnostic> diagnostics, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, BoundBlockStatement statement)
    {
        Diagnostics = diagnostics;
        Functions = functions;
        Statement = statement;
    }

    public BoundGlobalScope GlobalScope
    {
        get;
    }
    public ImmutableArray<Diagnostic> Diagnostics
    {
        get;
    }
    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> Functions
    {
        get;
    }
    public BoundBlockStatement Statement
    {
        get;
    }
}
