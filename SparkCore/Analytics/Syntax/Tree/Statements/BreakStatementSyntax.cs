namespace SparkCore.Analytics.Syntax.Tree.Statements;

public sealed class BreakStatementSyntax : StatementSyntax
{
    public BreakStatementSyntax(SyntaxToken keyword)
    {
        Keyword = keyword;
    }
    public override SyntaxKind Kind => SyntaxKind.BreakStatement;
    public SyntaxToken Keyword
    {
        get;
    }
}