using SparkCore.Analytics.Binding.Tree.Expressions;

namespace SparkCore.Analytics.Binding.Tree.Statements;

internal sealed class BoundDoWhileStatement : BoundStatement
{
    public BoundDoWhileStatement(BoundStatement body, BoundExpression condition)
    {
        Body = body;
        Condition = condition;
    }

    public override BoundNodeKind Kind => BoundNodeKind.DoWhileStatement;

    public BoundStatement Body
    {
        get;
    }
    public BoundExpression Condition
    {
        get;
    }
}
