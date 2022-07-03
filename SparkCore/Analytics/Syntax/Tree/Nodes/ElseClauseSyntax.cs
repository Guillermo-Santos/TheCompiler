using SparkCore.Analytics.Syntax.Tree.Statements;

namespace SparkCore.Analytics.Syntax.Tree.Nodes
{
    public sealed class ElseClauseSyntax : SyntaxNode
    {
        public ElseClauseSyntax(SyntaxToken elseKeyword, SyntaxStatement elseStatement)
        {
            ElseKeyword = elseKeyword;
            ElseStatement = elseStatement;
        }

        public SyntaxToken ElseKeyword { get; }
        public SyntaxStatement ElseStatement { get; }

        public override SyntaxKind Kind => SyntaxKind.ElseClause;
    }
}
