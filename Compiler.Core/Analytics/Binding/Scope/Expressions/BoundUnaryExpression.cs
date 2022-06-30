using System;

namespace SparkCore.Analytics.Binding.Scope.Expressions
{
    internal sealed class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression(BoundUnaryOperator op, BoundExpression operand)
        {
            Op = op;
            Operand = operand;
        }
        public override BoundNodeType NodeType => BoundNodeType.UnaryExpression;
        public override Type Type => Op.Type;
        public BoundUnaryOperator Op { get; }
        public BoundExpression Operand { get; }

    }

}
