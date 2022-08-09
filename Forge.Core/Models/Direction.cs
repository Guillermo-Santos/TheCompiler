namespace Forge.Core.Models;

public abstract class Direction
{
    public Direction(string path)
    {
        Path = path;
    }
    public string Path
    {
        get;
        set;
    }
}