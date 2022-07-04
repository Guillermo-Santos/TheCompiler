using SparkCore.Analytics.Binding.Tree.Expressions;

namespace SparkCore.Analytics.Binding.Tree.Statements
{
    internal sealed class BoundWhileStatement : BoundStatement
    {
        public BoundWhileStatement(BoundExpression condition, BoundStatement body)
        {
            Condition = condition;
            Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
        public BoundExpression Condition
        {
            get;
        }
        public BoundStatement Body
        {
            get;
        }

    }
}
