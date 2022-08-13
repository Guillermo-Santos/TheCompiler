using SparkCore.IO.Text;

namespace Forge.Core.Models;
public sealed class Document : Direction
{
    public string FileName { get; }
    public string Text
    {
        get;
        set;
    }
    public Document(string filePath, string text): base(System.IO.Path.GetDirectoryName(filePath))
    {
        FileName = System.IO.Path.GetFileName(filePath);
        Text = text;
    }
    public SourceText SourceText => SourceText.From(Text, Path);
}