using System.Collections.Generic;

namespace SparkCore.Analytics.Syntax.Tree.Expressions
{
    public sealed class UnarySyntaxExpression : SyntaxExpression
    {
        public UnarySyntaxExpression(SyntaxToken operatorToken, SyntaxExpression operand)
        {
            OperatorToken = operatorToken;
            Operand = operand;
        }
        public override SyntaxType Type => SyntaxType.UnaryExpression;

        public SyntaxToken OperatorToken { get; }
        public SyntaxExpression Operand { get; }
    }
}
