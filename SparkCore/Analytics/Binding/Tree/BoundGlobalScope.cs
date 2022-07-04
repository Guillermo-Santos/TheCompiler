using System.Collections.Immutable;
using SparkCore.Analytics.Binding.Scope.Statements;
using SparkCore.Analytics.Diagnostics;

namespace SparkCore.Analytics.Binding.Scope
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope(BoundGlobalScope previous, ImmutableArray<Diagnostic> diagnostics, ImmutableArray<VariableSymbol> variables, BoundStatement statement)
        {
            Previous = previous;
            Diagnostics = diagnostics;
            Variables = variables;
            Statement = statement;
        }

        public BoundGlobalScope Previous
        {
            get;
        }
        public ImmutableArray<Diagnostic> Diagnostics
        {
            get;
        }
        public ImmutableArray<VariableSymbol> Variables
        {
            get;
        }
        public BoundStatement Statement
        {
            get;
        }
    }
}
