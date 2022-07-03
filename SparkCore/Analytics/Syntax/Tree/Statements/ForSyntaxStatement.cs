using SparkCore.Analytics.Syntax.Tree.Expressions;

namespace SparkCore.Analytics.Syntax.Tree.Statements
{
    public sealed class ForSyntaxStatement : SyntaxStatement
    {
        public ForSyntaxStatement(SyntaxToken keyword, SyntaxToken identifier, SyntaxToken equalsToken, SyntaxExpression lowerBound, SyntaxToken toKeyword, SyntaxExpression upperBound, SyntaxStatement body)
        {
            Keyword = keyword;
            Identifier = identifier;
            EqualsToken = equalsToken;
            LowerBound = lowerBound;
            ToKeyword = toKeyword;
            UpperBound = upperBound;
            Body = body;
        }
        public override SyntaxKind Kind => SyntaxKind.ForStatement;

        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualsToken { get; }
        public SyntaxExpression LowerBound { get; }
        public SyntaxToken ToKeyword { get; }
        public SyntaxExpression UpperBound { get; }
        public SyntaxStatement Body { get; }
    }

}
