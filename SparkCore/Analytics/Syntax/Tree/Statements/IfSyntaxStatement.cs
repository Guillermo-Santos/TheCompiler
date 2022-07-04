using SparkCore.Analytics.Syntax.Tree.Expressions;
using SparkCore.Analytics.Syntax.Tree.Nodes;

namespace SparkCore.Analytics.Syntax.Tree.Statements
{
    public sealed class IfSyntaxStatement : SyntaxStatement
    {
        public IfSyntaxStatement(SyntaxToken ifKeyword, SyntaxExpression condition, SyntaxStatement thenStatement, ElseClauseSyntax elseClause)
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
        public SyntaxExpression Condition
        {
            get;
        }
        public SyntaxStatement ThenStatement
        {
            get;
        }
        public ElseClauseSyntax ElseClause
        {
            get;
        }

    }
}
