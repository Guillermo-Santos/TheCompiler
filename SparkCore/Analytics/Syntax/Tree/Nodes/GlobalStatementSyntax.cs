using SparkCore.Analytics.Syntax.Tree.Statements;

namespace SparkCore.Analytics.Syntax.Tree.Nodes;

public sealed class GlobalStatementSyntax : MemberSyntax
{
    public GlobalStatementSyntax(StatementSyntax statement)
    {
        Statement = statement;
    }

    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;

    public StatementSyntax Statement
    {
        get;
    }
}
