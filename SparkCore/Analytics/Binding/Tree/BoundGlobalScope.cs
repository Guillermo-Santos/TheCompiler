using System.Collections.Immutable;
using SparkCore.Analytics.Binding.Tree.Statements;
using SparkCore.Analytics.Symbols;
using SparkCore.IO.Diagnostics;

namespace SparkCore.Analytics.Binding.Tree;
internal sealed class BoundGlobalScope
{
    public BoundGlobalScope(BoundGlobalScope? previous, ImmutableArray<Diagnostic> diagnostics, FunctionSymbol? mainFunction, FunctionSymbol? scriptFunction, ImmutableArray<FunctionSymbol> functions, ImmutableArray<VariableSymbol> variables, ImmutableArray<BoundStatement> statements)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        MainFunction = mainFunction;
        ScriptFunction = scriptFunction;
        Functions = functions;
        Variables = variables;
        Statements = statements;
    }

    public BoundGlobalScope? Previous
    {
        get;
    }
    public ImmutableArray<Diagnostic> Diagnostics
    {
        get;
    }
    public FunctionSymbol? MainFunction
    {
        get;
    }
    public FunctionSymbol? ScriptFunction
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
