using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree.Expressions
{
    internal sealed class BoundErrorExpression : BoundExpression
    {
        public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;
        public override TypeSymbol Type => TypeSymbol.Error;

    }
}
