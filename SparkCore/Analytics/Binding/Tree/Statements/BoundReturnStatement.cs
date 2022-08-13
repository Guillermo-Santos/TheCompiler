using SparkCore.Analytics.Binding.Tree.Expressions;

namespace SparkCore.Analytics.Binding.Tree.Statements;

internal sealed class BoundReturnStatement : BoundStatement
{
    public BoundReturnStatement(BoundExpression? expression)
    {
        Expression = expression;
    }

    public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;
    public BoundExpression? Expression
    {
        get;
    }
}
