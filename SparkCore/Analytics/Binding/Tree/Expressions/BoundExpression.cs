using System;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree.Expressions;

internal abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type
    {
        get;
    }

    public virtual BoundConstant ConstantValue
    {
        get;
    }
}
