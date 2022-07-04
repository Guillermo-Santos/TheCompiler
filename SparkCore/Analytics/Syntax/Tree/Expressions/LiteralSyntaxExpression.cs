namespace SparkCore.Analytics.Syntax.Tree.Expressions
{
    public sealed class LiteralSyntaxExpression : SyntaxExpression
    {
        public LiteralSyntaxExpression(SyntaxToken literalToken)
            : this(literalToken, literalToken.Value)
        {
        }
        public LiteralSyntaxExpression(SyntaxToken literalToken, object value)
        {
            LiteralToken = literalToken;
            Value = value;
        }
        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
        public SyntaxToken LiteralToken
        {
            get;
        }
        public object Value
        {
            get;
        }
    }
}
