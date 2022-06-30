using SparkCore.Analytics;
using SparkCore.Analytics.Binding;
using SparkCore.Analytics.Binding.Scope;
using SparkCore.Analytics.Diagnostics;
using SparkCore.Analytics.Syntax.Tree;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace SparkCore
{
    public class Compilation
    {
        private BoundGlobalScope _globalScope;
        public Compilation Previous { get; }
        public SyntaxTree SyntaxTree { get; }
        public Compilation(SyntaxTree syntaxTree)
            : this(null, syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }
        private Compilation(Compilation previous, SyntaxTree syntaxTree)
        {
            Previous = previous;
            SyntaxTree = syntaxTree;
        }

        internal BoundGlobalScope GlobalScope
        {
            get
            {
                if(_globalScope == null)
                {
                    var globalScope = Binder.BindGlobalScope(Previous?.GlobalScope,SyntaxTree.Root);
                    Interlocked.CompareExchange(ref _globalScope, globalScope, null);
                }
                return _globalScope;
            }
        }

        public Compilation ContinueWith(SyntaxTree syntaxTree)
        {
            return new Compilation(this, syntaxTree);
        }

        public EvaluationResult Evaluate(Dictionary<VariableSymbol, object> variables)
        {
            var diagnostics = SyntaxTree.Diagnostics.Concat(GlobalScope.Diagnostics).ToImmutableArray();
            if (diagnostics.Any())
                return new EvaluationResult(diagnostics, null);
            

            var evaluator = new Evaluator(GlobalScope.Statement, variables);
            var value = evaluator.Evaluate();
            return new EvaluationResult(ImmutableArray<Diagnostic>.Empty, value);
        }
    }
}
