namespace SparkCore.Analytics.Binding.Tree.Statements
{
    internal sealed class BoundLabelStatement : BoundStatement
    {
        public BoundLabelStatement(BoundLabel label)
        {
            Label = label;
        }

        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;

        public BoundLabel Label
        {
            get;
        }
    }

}
