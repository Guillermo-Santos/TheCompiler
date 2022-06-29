using System.Collections.Generic;

namespace SparkCore.Analytics.Syntax.Expressions
{
    public sealed class BinarySyntaxExpression : SyntaxExpression
    {
        public BinarySyntaxExpression(SyntaxExpression left, SyntaxToken operatorToken, SyntaxExpression right)
        {
            Left = left;
            OperatorToken = operatorToken;
            Right = right;
        }
        public override SyntaxType Type => SyntaxType.BinaryExpression;

        public SyntaxExpression Left { get; }
        public SyntaxToken OperatorToken { get; }
        public SyntaxExpression Right { get; }
    }
}
