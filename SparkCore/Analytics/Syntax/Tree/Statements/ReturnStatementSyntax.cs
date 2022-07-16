using SparkCore.Analytics.Syntax.Tree.Expressions;

namespace SparkCore.Analytics.Syntax.Tree.Statements;

public sealed class ReturnStatementSyntax : StatementSyntax
{
    public ReturnStatementSyntax(SyntaxTree syntaxTree, SyntaxToken returnKeyword, ExpressionSyntax expression)
        : base(syntaxTree)
    {
        ReturnKeyword = returnKeyword;
        Expression = expression;
    }

    public override SyntaxKind Kind => SyntaxKind.ReturnStatement;

    public SyntaxToken ReturnKeyword
    {
        get;
    }
    public ExpressionSyntax Expression
    {
        get;
    }
}

