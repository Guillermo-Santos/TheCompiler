namespace SparkCore.Analytics.Syntax.Tree.Expressions
{
    public sealed class AssignmentSyntaxExpression : ExpressionSyntax
    {
        public AssignmentSyntaxExpression(SyntaxToken identifierToken, SyntaxToken equalsToken, ExpressionSyntax expression)
        {
            IdentifierToken = identifierToken;
            EqualsToken = equalsToken;
            Expression = expression;
        }
        public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;

        public SyntaxToken IdentifierToken
        {
            get;
        }
        public SyntaxToken EqualsToken
        {
            get;
        }
        public ExpressionSyntax Expression
        {
            get;
        }
    }
}
