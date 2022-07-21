using SparkCore.Analytics.Syntax.Tree.Expressions;

namespace SparkCore.Analytics.Syntax.Tree.Statements;

public sealed partial class DoWhileStatementSyntax : StatementSyntax
{
    public DoWhileStatementSyntax(SyntaxTree syntaxTree, SyntaxToken doKeyword, StatementSyntax body, SyntaxToken whileKeyword, ExpressionSyntax condition) : base(syntaxTree)
    {
        DoKeyword = doKeyword;
        Body = body;
        WhileKeyword = whileKeyword;
        Condition = condition;
    }

    public override SyntaxKind Kind => SyntaxKind.DoWhileStatement;

    public SyntaxToken DoKeyword
    {
        get;
    }
    public StatementSyntax Body
    {
        get;
    }
    public SyntaxToken WhileKeyword
    {
        get;
    }
    public ExpressionSyntax Condition
    {
        get;
    }
}

