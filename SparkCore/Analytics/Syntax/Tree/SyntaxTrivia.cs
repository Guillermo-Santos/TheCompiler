using SparkCore.IO.Text;

namespace SparkCore.Analytics.Syntax.Tree;

public sealed class SyntaxTrivia
{
    public SyntaxTrivia(SyntaxTree syntaxTree, SyntaxKind kind, int position, string text)
    {
        SyntaxTree = syntaxTree;
        Kind = kind;
        Position = position;
        Text = text;
    }
    public SyntaxTree SyntaxTree
    {
        get;
    }
    public SyntaxKind Kind
    {
        get;
    }
    public int Position
    {
        get;
    }
    public TextSpan Span => new(Position, Text?.Length ?? 0);
    public string Text
    {
        get;
    }
}
