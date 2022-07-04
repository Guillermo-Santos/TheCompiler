using System;
using SparkCore.Analytics.Symbols;

namespace SparkCore.Analytics.Binding.Tree.Expressions
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression operand)
        {
            Op = op;
            Operand = operand;
        }
        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override TypeSymbol Type => Op.Type;
        public BoundUnaryOperator Op
        {
            get;
        }
        public BoundExpression Operand
        {
            get;
        }

    }

}
