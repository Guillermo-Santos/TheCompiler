using SparkCore.Analytics.Syntax.Tree.Expressions;
using SparkCore.Analytics.Syntax.Tree.Nodes;

namespace SparkCore.Analytics.Syntax.Tree.Statements;

public sealed class VariableDeclarationStatementSyntax : StatementSyntax
{
    public VariableDeclarationStatementSyntax(SyntaxTree syntaxTree, SyntaxToken keyword, SyntaxToken identifier, TypeClauseSyntax typeClause, SyntaxToken equalsToken, ExpressionSyntax initializer)
        : base(syntaxTree)
    {
        Keyword = keyword;
        Identifier = identifier;
        TypeClause = typeClause;
        EqualsToken = equalsToken;
        Initializer = initializer;
    }
    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;
    public SyntaxToken Keyword
    {
        get;
    }
    public SyntaxToken Identifier
    {
        get;
    }
    public TypeClauseSyntax TypeClause
    {
        get;
    }
    public SyntaxToken EqualsToken
    {
        get;
    }
    public ExpressionSyntax Initializer
    {
        get;
    }
}
