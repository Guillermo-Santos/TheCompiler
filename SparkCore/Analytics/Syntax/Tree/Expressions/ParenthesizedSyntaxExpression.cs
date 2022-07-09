namespace SparkCore.Analytics.Syntax.Tree.Expressions
{
    public sealed class ParenthesizedSyntaxExpression : ExpressionSyntax
    {
        public ParenthesizedSyntaxExpression(SyntaxToken openParenthesisToken, ExpressionSyntax expression, SyntaxToken closeParenthesisToken)
        {
            OpenParenthesisToken = openParenthesisToken;
            Expression = expression;
            CloseParenthesisToken = closeParenthesisToken;
        }
        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

        public SyntaxToken OpenParenthesisToken
        {
            get;
        }
        public ExpressionSyntax Expression
        {
            get;
        }
        public SyntaxToken CloseParenthesisToken
        {
            get;
        }
    }
}
