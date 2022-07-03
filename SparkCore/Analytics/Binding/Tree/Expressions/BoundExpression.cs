using System;

namespace SparkCore.Analytics.Binding.Scope.Expressions
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract Type Type { get; }
    }
}
