using System.Collections.Generic;

namespace Compiler.Core.Syntax.Expressions
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

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenParenthesisToken;
            yield return Expression;
            yield return CloseParenthesisToken;
        }
    }
}
