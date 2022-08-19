using SparkCore.Analytics.Syntax.Tree.Statements;

namespace SparkCore.Analytics.Syntax.Tree.Nodes;

public sealed partial class GlobalStatementSyntax : MemberSyntax
{
    public GlobalStatementSyntax(SyntaxTree syntaxTree, StatementSyntax statement) : base(syntaxTree)
    {
        Statement = statement;
    }

    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;

    public StatementSyntax Statement
    {
        get;
    }
}
