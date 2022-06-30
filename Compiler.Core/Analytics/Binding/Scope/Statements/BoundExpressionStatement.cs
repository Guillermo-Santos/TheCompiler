using SparkCore.Analytics.Binding.Scope.Expressions;

namespace SparkCore.Analytics.Binding.Scope.Statements
{
    internal sealed class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement(BoundExpression expression)
        {
            Expression = expression;
        }

        public override BoundNodeType NodeType => BoundNodeType.ExpressionStatement;
        public BoundExpression Expression { get; }
    }
}
