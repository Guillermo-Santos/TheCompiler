namespace SparkCore.Analytics.Syntax.Tree.Expressions
{
    public sealed class UnarySyntaxExpression : ExpressionSyntax
    {
        public UnarySyntaxExpression(SyntaxToken operatorToken, ExpressionSyntax operand)
        {
            OperatorToken = operatorToken;
            Operand = operand;
        }
        public override SyntaxKind Kind => SyntaxKind.UnaryExpression;

        public SyntaxToken OperatorToken
        {
            get;
        }
        public ExpressionSyntax Operand
        {
            get;
        }
    }
}
