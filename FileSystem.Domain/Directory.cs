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

    public bool Remove(FileSystemNode node)
    {
        ArgumentNullException.ThrowIfNull(node);

        var index = _children.IndexOf(node);
        if (index < 0)
        {
            return false;
        }

        _children.RemoveAt(index);
        node.Detach();
        return true;
    }

    public void Insert(int index, FileSystemNode node)
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

        _children.Insert(index, node);
        node.AttachTo(this);
    }

    public int IndexOf(FileSystemNode node) => _children.IndexOf(node);

    public void Sort(ISortStrategy strategy, SortDirection direction)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        strategy.Sort(this, direction);
    }

    internal void SortInPlace(Comparison<FileSystemNode> comparison)
    {
        _children.Sort(comparison);
    }

    internal void RestoreChildrenOrder(IReadOnlyList<FileSystemNode> order)
    {
        ArgumentNullException.ThrowIfNull(order);

        if (order.Count != _children.Count || order.Any(child => !_children.Contains(child)))
        {
            throw new InvalidOperationException("Restored order must contain the same children.");
        }

        _children.Clear();
        foreach (var child in order)
        {
            _children.Add(child);
        }
    }

    public override void Accept(IFsVisitor visitor) => visitor.VisitDirectory(this);

    public override long CalculateSize() => _children.Sum(child => child.CalculateSize());

    public override FileSystemNode Clone()
    {
        var copy = new Directory(Name, EnglishName, _xmlTagOverride);
        CopyTagsTo(copy);
        foreach (var child in _children)
        {
            copy.Add(child.Clone());
        }

        return copy;
    }

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
