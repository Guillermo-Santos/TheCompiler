using Compiler.Core.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace Compiler.Core.Syntax
{
    public sealed class SyntaxToken : SyntaxNode
    {
        public SyntaxToken(SyntaxType type, int position, string text, object value)
        {
            Type = type;
            Position = position;
            Text = text;
            Value = value;
        }

        public override SyntaxType Type { get; }
        public int Position { get; }
        public string Text { get; }
        public object Value { get; }
        public TextSpan Span => new TextSpan(Position, Text.Length);
        public override IEnumerable<SyntaxNode> GetChildren()
        {
            return Enumerable.Empty<SyntaxNode>();
        }
    }
}
