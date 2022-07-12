﻿using SparkCore.Analytics.Syntax.Tree.Expressions;

namespace SparkCore.Analytics.Syntax.Tree.Statements
{
    public sealed class WhileStatementSyntax : StatementSyntax
    {
        public WhileStatementSyntax(SyntaxToken keyword, ExpressionSyntax condition, StatementSyntax body)
        {
            Keyword = keyword;
            Condition = condition;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.WhileStatement;
        public SyntaxToken Keyword
        {
            get;
        }
        public ExpressionSyntax Condition
        {
            get;
        }
        public StatementSyntax Body
        {
            get;
        }

    }

}