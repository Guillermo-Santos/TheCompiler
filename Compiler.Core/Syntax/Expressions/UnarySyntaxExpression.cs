using System.Collections.Generic;

namespace Compiler.Core.Syntax.Expressions
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

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OperatorToken;
            yield return Operand;
        }
    }
}
