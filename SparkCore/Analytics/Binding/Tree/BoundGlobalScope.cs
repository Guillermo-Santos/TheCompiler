using System.Collections.Immutable;
using SparkCore.Analytics.Binding.Tree.Statements;
using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree;
internal sealed class BoundGlobalScope
{
    public BoundGlobalScope(BoundGlobalScope previous, ImmutableArray<Diagnostic> diagnostics, ImmutableArray<FunctionSymbol> functions, ImmutableArray<VariableSymbol> variables, ImmutableArray<BoundStatement> statements)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        Functions = functions;
        Variables = variables;
        Statements = statements;
    }

    public BoundGlobalScope Previous
    {
        get;
    }
    public ImmutableArray<Diagnostic> Diagnostics
    {
        get;
    }
    public ImmutableArray<FunctionSymbol> Functions
    {
        get;
    }
    public ImmutableArray<VariableSymbol> Variables
    {
        get;
    }
    public ImmutableArray<BoundStatement> Statements
    {
        get;
    }
}
