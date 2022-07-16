using SparkCore.Analytics.Syntax.Tree.Statements;

namespace SparkCore.Analytics.Syntax.Tree.Nodes;

public sealed class ElseClauseSyntax : SyntaxNode
{
    public ElseClauseSyntax(SyntaxTree syntaxTree, SyntaxToken elseKeyword, StatementSyntax elseStatement) : base(syntaxTree)
    {
        ElseKeyword = elseKeyword;
        ElseStatement = elseStatement;
    }

    public SyntaxToken ElseKeyword
    {
        get;
    }
    public StatementSyntax ElseStatement
    {
        get;
    }

    public override SyntaxKind Kind => SyntaxKind.ElseClause;
}