using SparkCore.IO.Text;

namespace SparkCore.Analytics.Syntax.Tree;

public sealed class SyntaxToken : SyntaxNode
{
    public SyntaxToken(SyntaxKind type, int position, string text, object value)
    {
        Kind = type;
        Position = position;
        Text = text;
        Value = value;
    }

    public override SyntaxKind Kind
    {
        get;
    }
    public int Position
    {
        get;
    }
    public string Text
    {
        get;
    }
    public object Value
    {
        get;
    }
    public override TextSpan Span => new(Position, Text?.Length ?? 0);

    public bool IsMissing => Text == null;
}
