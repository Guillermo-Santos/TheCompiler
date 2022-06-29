using System.Collections.Generic;

namespace SparkCore.Analytics.Syntax.Expressions
{
    public sealed class ParenthesizedSyntaxExpression : SyntaxExpression
    {
        public ParenthesizedSyntaxExpression(SyntaxToken openParenthesisToken, SyntaxExpression expression, SyntaxToken closeParenthesisToken)
        {
            OpenParenthesisToken = openParenthesisToken;
            Expression = expression;
            CloseParenthesisToken = closeParenthesisToken;
        }
        public override SyntaxType Type => SyntaxType.ParenthesizedExpression;

        public SyntaxToken OpenParenthesisToken { get; }
        public SyntaxExpression Expression { get; }
        public SyntaxToken CloseParenthesisToken { get; }
    }
}
