using System.Collections.Immutable;

namespace SparkCore.Analytics.Syntax.Tree.Statements
{
    public sealed class BlockSyntaxStatement : SyntaxStatement
    {
        public BlockSyntaxStatement(SyntaxToken openBraceToken, ImmutableArray<SyntaxStatement> statements, SyntaxToken closeBraceToken)
        {
            OpenBraceToken = openBraceToken;
            Statements = statements;
            CloseBraceToken = closeBraceToken;
        }

        public override SyntaxKind Kind => SyntaxKind.BlockStatement;
        public SyntaxToken OpenBraceToken
        {
            get;
        }
        public ImmutableArray<SyntaxStatement> Statements
        {
            get;
        }
        public SyntaxToken CloseBraceToken
        {
            get;
        }

    }
}
