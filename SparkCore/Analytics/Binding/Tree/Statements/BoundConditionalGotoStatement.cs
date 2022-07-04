using SparkCore.Analytics.Binding.Tree.Expressions;

namespace SparkCore.Analytics.Binding.Tree.Statements;

internal sealed class BoundConditionalGotoStatement : BoundStatement
{
    public BoundConditionalGotoStatement(BoundLabel label, BoundExpression condition, bool jumpIfTrue = true)
    {
        Label = label;
        Condition = condition;
        JumpIfTrue = jumpIfTrue;
    }

    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;

    public BoundLabel Label
    {
        get;
    }
    public BoundExpression Condition
    {
        get;
    }
    public bool JumpIfTrue
    {
        get;
    }
}

