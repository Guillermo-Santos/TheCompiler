namespace SparkCore.Analytics.Syntax.Tree.Expressions
{
    public sealed class NameSyntaxExpression : SyntaxExpression
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
