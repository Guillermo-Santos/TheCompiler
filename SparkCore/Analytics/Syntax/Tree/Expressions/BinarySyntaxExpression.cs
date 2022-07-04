namespace SparkCore.Analytics.Syntax.Tree.Expressions
{
    public sealed class BinarySyntaxExpression : SyntaxExpression
    {
        public BinarySyntaxExpression(SyntaxExpression left, SyntaxToken operatorToken, SyntaxExpression right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }
        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

        public SyntaxExpression Left
        {
            get;
        }
        public SyntaxToken OperatorToken
        {
            get;
        }
        public SyntaxExpression Right
        {
            get;
        }
    }
}
