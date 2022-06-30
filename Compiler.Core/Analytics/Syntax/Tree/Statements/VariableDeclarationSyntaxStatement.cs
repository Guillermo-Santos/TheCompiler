using SparkCore.Analytics.Syntax.Tree.Expressions;

namespace SparkCore.Analytics.Syntax.Tree.Statements
{
    public sealed class VariableDeclarationSyntaxStatement : SyntaxStatement
    {
        public VariableDeclarationSyntaxStatement(SyntaxToken keyword, SyntaxToken identifier, SyntaxToken equalsToken, SyntaxExpression initializer)
        {
            Keyword = keyword;
            Identifier = identifier;
            EqualsToken = equalsToken;
            Initializer = initializer;
        }
        public override SyntaxType Type => SyntaxType.VariableDeclarationStatement;
        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualsToken { get; }
        public SyntaxExpression Initializer { get; }
    }
}
