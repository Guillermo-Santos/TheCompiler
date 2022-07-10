using SparkCore.Analytics.Syntax.Tree.Expressions;

namespace SparkCore.Analytics.Syntax.Tree.Statements;

public sealed class DoWhileSyntaxStatement : StatementSyntax
{
    public DoWhileSyntaxStatement(SyntaxToken doKeyword, StatementSyntax body, SyntaxToken whileKeyword, ExpressionSyntax condition)
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

