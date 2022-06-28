using Compiler.Core.Binding;
using Compiler.Core.Diagnostics;
using Compiler.Core.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Core
{
    public class Compilation
    {
        public Compilation(SyntaxTree syntax)
        {
            Syntax = syntax;
        }
        public SyntaxTree Syntax { get; }
        public EvaluationResult Evaluate(Dictionary<VariableSymbol,object> variables)
        {
            var binder = new Binder(variables);
            var boundExpressio = binder.BindExpression(Syntax.Root);
            var diagnostics = Syntax.Diagnostics.Concat(binder.Diagnostics).ToArray();

            if (diagnostics.Any())
            {
                return new EvaluationResult(diagnostics, null);
            }

            var evaluator = new Evaluator(boundExpressio,variables);
            var value = evaluator.Evaluate();
            return new EvaluationResult(Array.Empty<Diagnostic>(), value);
        }
    }
}
