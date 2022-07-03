using SparkCore.Analytics.Syntax.Tree.Expressions;

namespace SparkCore.Analytics.Syntax.Tree.Statements
{
    public sealed class WhileSyntaxStatement : SyntaxStatement
    {
        public WhileSyntaxStatement(SyntaxToken keyword, SyntaxExpression condition, SyntaxStatement body)
        {
            Keyword = keyword;
            Condition = condition;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.WhileStatement;
        public SyntaxToken Keyword { get; }
        public SyntaxExpression Condition { get; }
        public SyntaxStatement Body { get; }

    }

}
