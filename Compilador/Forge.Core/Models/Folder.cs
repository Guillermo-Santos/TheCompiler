using System.Collections.Immutable;

namespace Forge.Core.Models;

public sealed class Folder : Direction
{
    public Folder(string path, ImmutableArray<Direction> directions) : base(path)
    {
        FolderName = new DirectoryInfo(path).Name;
        Directions = directions;
    }
    public string FolderName
    {
        get;
    }
    public ImmutableArray<Direction> Directions
    {
        get;
    }
}