namespace SparkCore.Analytics.Syntax.Tree.Expressions
{
    public sealed class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken operatorToken, ExpressionSyntax operand) : base(syntaxTree)
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
