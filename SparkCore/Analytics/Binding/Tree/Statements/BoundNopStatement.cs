namespace SparkCore.Analytics.Binding.Tree.Statements;

internal sealed class BoundNopStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.NopStatement;
}
