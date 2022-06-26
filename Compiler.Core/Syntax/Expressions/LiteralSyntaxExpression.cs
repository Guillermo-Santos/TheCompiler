using Compiler.Core.Syntax;
using System.Collections.Generic;

namespace Compiler.Core.Syntax.Expressions
{
    public sealed class LiteralSyntaxExpression : SyntaxExpression
    {
        public LiteralSyntaxExpression(SyntaxToken literalToken)
            : this(literalToken, literalToken.Value)
        {
        }
        public LiteralSyntaxExpression(SyntaxToken literalToken, object value)
        {
            LiteralToken = literalToken;
            Value = value;
        }
        public override SyntaxType Type => SyntaxType.LiteralExpression;
        public SyntaxToken LiteralToken { get; }
        public object Value { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LiteralToken;
        }
    }
}
