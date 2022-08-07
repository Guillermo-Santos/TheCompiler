using System.IO;

namespace SparkCore.Analytics.Binding.Tree;

internal abstract class BoundNode
{
    public abstract BoundNodeKind Kind
    {
        get;
    }
    public override string ToString()
    {
        using var writter = new StringWriter();
        this.WriteTo(writter);
        return writter.ToString();
    }
}

