using SparkCore.Analytics.Syntax.Tree.Expressions;
using SparkCore.Analytics.Syntax.Tree.Nodes;

namespace SparkCore.Analytics.Syntax.Tree.Statements;

public sealed partial class IfStatementSyntax : StatementSyntax
{
    public IfStatementSyntax(SyntaxTree syntaxTree, SyntaxToken ifKeyword, ExpressionSyntax condition, StatementSyntax thenStatement, ElseClauseSyntax? elseClause) : base(syntaxTree)
    {
        IfKeyword = ifKeyword;
        Condition = condition;
        ThenStatement = thenStatement;
        ElseClause = elseClause;
    }

    public override SyntaxKind Kind => SyntaxKind.IfStatement;
    public SyntaxToken IfKeyword
    {
        get;
    }
    public ExpressionSyntax Condition
    {
        get;
    }
    public StatementSyntax ThenStatement
    {
        get;
    }
    public ElseClauseSyntax? ElseClause
    {
        get;
    }

}
