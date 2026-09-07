namespace FileSystem.Domain;

public abstract class FileSystemNode
{
    private readonly HashSet<Tag> _tags = [];

    protected FileSystemNode(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }

        Id = Guid.NewGuid();
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; }

    public string Name { get; }

    public Directory? Parent { get; private set; }

    public DateTime CreatedAt { get; }

    public IReadOnlyCollection<Tag> Tags => _tags;

    public abstract void Accept(IFsVisitor visitor);

    public abstract long CalculateSize();

    public abstract FileSystemNode Clone();

    public string GetFullPath(string separator = "/")
    {
        var parts = new List<string>();
        for (FileSystemNode? current = this; current is not null; current = current.Parent)
        {
            parts.Add(current.Name);
        }

        parts.Reverse();
        return string.Join(separator, parts);
    }

    internal void AttachTo(Directory parent)
    {
        Parent = parent;
    }

    internal void Detach()
    {
        Parent = null;
    }
}
