namespace SparkCore.Analytics.Syntax.Tree.Expressions
{
    public sealed class NameSyntaxExpression : ExpressionSyntax
    {
        public NameSyntaxExpression(SyntaxToken identifierToken)
        {
            IdentifierToken = identifierToken;
        }
        public override SyntaxKind Kind => SyntaxKind.NameExpression;

        public SyntaxToken IdentifierToken
        {
            get;
        }
    }
}
