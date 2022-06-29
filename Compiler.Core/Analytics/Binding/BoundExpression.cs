using System;

namespace SparkCore.Analytics.Binding
{
    internal abstract class BoundExpression : BoundNode
    {
        public abstract Type Type { get; }
    }

}
