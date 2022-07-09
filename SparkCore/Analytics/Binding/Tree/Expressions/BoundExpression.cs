using System;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree.Expressions;

internal abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type
    {
        get;
    }
}


internal sealed class BoundConversionExpression : BoundExpression
{
    public BoundConversionExpression(TypeSymbol type, BoundExpression expression)
    {
        Type = type;
        Expression = expression;
    }
    public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
    public override TypeSymbol Type { get; }
    public BoundExpression Expression { get; }

}
