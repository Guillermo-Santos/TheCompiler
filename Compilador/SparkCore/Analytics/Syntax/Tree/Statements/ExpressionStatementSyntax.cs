using SparkCore.Analytics.Syntax.Tree.Expressions;

namespace SparkCore.Analytics.Syntax.Tree.Statements
{
    public sealed partial class ExpressionStatementSyntax : StatementSyntax
    {
        public ExpressionStatementSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression) : base(syntaxTree)
        {
            Expression = expression;
        }

        public ExpressionSyntax Expression
        {
            get;
        }

        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
    }
}
