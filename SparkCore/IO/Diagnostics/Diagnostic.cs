using SparkCore.IO.Text;

namespace SparkCore.IO.Diagnostics;

public sealed class Diagnostic
{
    public Diagnostic(TextLocation location, string message)
    {
        Location = location;
        Message = message;
    }

    public TextLocation Location
    {
        get;
    }
    public string Message
    {
        get;
    }
    public string FullMessage => $"{Location.FileName}({Location.StartLine + 1}, {Location.StartCharacter + 1},{Location.EndLine + 1}, {Location.EndCharacter + 1}): {Message}";
    public override string ToString() => Message;
}
