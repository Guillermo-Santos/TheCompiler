using System.Collections.Generic;

namespace Compiler.Core.Syntax.Expressions
{
    public sealed class NameSyntaxExpression : SyntaxExpression
    {
        public NameSyntaxExpression(SyntaxToken identifierToken)
        {
            IdentifierToken = identifierToken;
        }
        public override SyntaxType Type => SyntaxType.NameExpression;

        public SyntaxToken IdentifierToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IdentifierToken;
        }
    }
}
