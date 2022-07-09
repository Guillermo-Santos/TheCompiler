using SparkCore.Analytics.Syntax.Tree.Expressions;
using SparkCore.Analytics.Syntax.Tree.Nodes;

namespace SparkCore.Analytics.Syntax.Tree.Statements
{
    public sealed class IfSyntaxStatement : StatementSyntax
    {
        public IfSyntaxStatement(SyntaxToken ifKeyword, ExpressionSyntax condition, StatementSyntax thenStatement, ElseClauseSyntax elseClause)
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
        public ElseClauseSyntax ElseClause
        {
            get;
        }

    }
}
