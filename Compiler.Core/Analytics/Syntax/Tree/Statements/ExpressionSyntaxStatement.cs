using SparkCore.Analytics.Syntax.Tree.Expressions;

namespace SparkCore.Analytics.Syntax.Tree.Statements
{
    public sealed class ExpressionSyntaxStatement : SyntaxStatement
    {
        public ExpressionSyntaxStatement(SyntaxExpression expression)
        {
            Expression = expression;
        }

        public SyntaxExpression Expression { get; }

        public override SyntaxType Type => SyntaxType.ExpressionStatement;
    }
}
