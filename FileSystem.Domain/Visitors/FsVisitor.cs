namespace FileSystem.Domain.Visitors;

public abstract class FsVisitor : IFsVisitor
{
    private readonly List<string> _trail = [];

    public IReadOnlyList<string> Trail => _trail;

    public abstract void VisitDirectory(Directory directory);

    public abstract void VisitWordFile(WordFile file);

    public abstract void VisitImageFile(ImageFile file);

    public abstract void VisitTextFile(TextFile file);

    protected void Record(FileSystemNode node)
    {
        var parentName = node.Parent?.Name ?? "(root)";
        _trail.Add($"Visiting: {parentName} -> {node.Name}");
    }

    protected void VisitChildren(Directory directory)
    {
        foreach (var child in directory.Children)
        {
            child.Accept(this);
        }
    }
}
