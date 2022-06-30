using System.Collections.Generic;

namespace SparkCore.Analytics.Syntax.Tree.Expressions
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
    }
}
