using SparkCore.Analytics.Text;
using System.Collections.Generic;
using System.Linq;

namespace SparkCore.Analytics.Syntax.Tree
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
        public override TextSpan Span => new TextSpan(Position, Text?.Length ?? 0);
    }
}
