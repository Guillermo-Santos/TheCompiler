using System.Collections.Generic;

namespace SparkCore.Analytics.Syntax.Tree.Expressions
{
    public sealed class ParenthesizedSyntaxExpression : SyntaxExpression
    {
        public ParenthesizedSyntaxExpression(SyntaxToken openParenthesisToken, SyntaxExpression expression, SyntaxToken closeParenthesisToken)
        {
            OpenParenthesisToken = openParenthesisToken;
            Expression = expression;
            CloseParenthesisToken = closeParenthesisToken;
        }
        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

        public SyntaxToken OpenParenthesisToken { get; }
        public SyntaxExpression Expression { get; }
        public SyntaxToken CloseParenthesisToken { get; }
    }
}
