using System.Collections.Generic;

namespace SparkCore.Analytics.Syntax.Tree.Expressions
{
    public sealed class NameSyntaxExpression : SyntaxExpression
    {
        public NameSyntaxExpression(SyntaxToken identifierToken)
        {
            IdentifierToken = identifierToken;
        }
        public override SyntaxType Type => SyntaxType.NameExpression;

        public SyntaxToken IdentifierToken { get; }
    }
}
