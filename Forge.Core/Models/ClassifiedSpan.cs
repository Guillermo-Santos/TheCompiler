using SparkCore.IO.Text;

namespace Forge.Core.Models;

public sealed class ClassifiedSpan
{
    public ClassifiedSpan(TextSpan span, Classification classification)
    {
        Span = span;
        Classification = classification;
    }

    public TextSpan Span
    {
        get;
    }
    public Classification Classification
    {
        get;
    }
}
