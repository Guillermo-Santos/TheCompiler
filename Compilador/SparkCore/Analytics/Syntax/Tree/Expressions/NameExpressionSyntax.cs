﻿namespace SparkCore.Analytics.Syntax.Tree.Expressions
{
    public sealed partial class NameExpressionSyntax : ExpressionSyntax
    {
        public NameExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken) : base(syntaxTree)
        {
            IdentifierToken = identifierToken;
        }
        public override SyntaxKind Kind => SyntaxKind.NameExpression;

        public SyntaxToken IdentifierToken
        {
            get;
        }
    }
}
