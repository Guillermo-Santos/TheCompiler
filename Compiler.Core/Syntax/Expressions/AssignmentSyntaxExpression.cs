using System.Collections.Generic;

namespace Compiler.Core.Syntax.Expressions
{
    public sealed class AssignmentSyntaxExpression : SyntaxExpression
    {
        public AssignmentSyntaxExpression(SyntaxToken identifierToken, SyntaxToken equalsToken, SyntaxExpression expression)
        {
            IdentifierToken = identifierToken;
            EqualsToken = equalsToken;
            Expression = expression;
        }
        public override SyntaxType Type => SyntaxType.AssignmentExpression;

        public SyntaxToken IdentifierToken { get; }
        public SyntaxToken EqualsToken { get; }
        public SyntaxExpression Expression { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IdentifierToken;
        }
    }
}
