﻿using System.Collections.Immutable;
using SparkCore.Analytics.Binding.Tree.Statements;
using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree
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