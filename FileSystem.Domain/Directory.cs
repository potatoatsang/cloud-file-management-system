namespace FileSystem.Domain;

public sealed class Directory : FileSystemNode
{
    private readonly List<FileSystemNode> _children = [];
    private readonly string? _xmlTagOverride;

    public Directory(string name, string? englishName = null, string? xmlTag = null)
        : base(name)
    {
        EnglishName = string.IsNullOrWhiteSpace(englishName) ? null : englishName;
        _xmlTagOverride = string.IsNullOrWhiteSpace(xmlTag) ? null : xmlTag;
    }

    public string? EnglishName { get; }

    public string DisplayName =>
        EnglishName is null ? Name : $"{Name} ({EnglishName})";

    public IReadOnlyList<FileSystemNode> Children => _children;

    public string XmlTag
    {
        get
        {
            if (_xmlTagOverride is not null)
            {
                return _xmlTagOverride.Replace(' ', '_');
            }

            var name = Name.Replace(' ', '_');
            if (EnglishName is null || Name == EnglishName)
            {
                return name;
            }

            return $"{name}_{EnglishName.Replace(' ', '_')}";
        }
    }

    public void Add(FileSystemNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (node.Parent is not null)
        {
            throw new InvalidOperationException("Node already belongs to a directory.");
        }

        if (WouldCreateCycle(node))
        {
            throw new InvalidOperationException("The tree must not contain cycles.");
        }

        _children.Add(node);
        node.AttachTo(this);
    }

    public override void Accept(IFsVisitor visitor) => visitor.VisitDirectory(this);

    public override long CalculateSize() => _children.Sum(child => child.CalculateSize());

    private bool WouldCreateCycle(FileSystemNode node)
    {
        if (node is not Directory)
        {
            return false;
        }

        for (Directory? current = this; current is not null; current = current.Parent)
        {
            if (ReferenceEquals(current, node))
            {
                return true;
            }
        }

        return false;
    }
}
