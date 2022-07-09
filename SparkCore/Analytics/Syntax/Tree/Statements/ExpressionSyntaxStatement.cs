﻿using SparkCore.Analytics.Syntax.Tree.Expressions;

namespace SparkCore.Analytics.Syntax.Tree.Statements
{
    public sealed class ExpressionSyntaxStatement : StatementSyntax
    {
        public ExpressionSyntaxStatement(ExpressionSyntax expression)
        {
            Expression = expression;
        }

        public ExpressionSyntax Expression
        {
            get;
        }

        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;
    }
}
