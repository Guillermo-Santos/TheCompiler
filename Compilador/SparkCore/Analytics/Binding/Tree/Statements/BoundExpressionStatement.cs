using SparkCore.Analytics.Binding.Tree.Expressions;

namespace SparkCore.Analytics.Binding.Tree.Statements
{
    internal sealed class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement(BoundExpression expression)
        {
            Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;
        public BoundExpression Expression
        {
            get;
        }
    }
}
