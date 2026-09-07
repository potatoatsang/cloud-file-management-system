namespace FileSystem.Domain.Commands;

public sealed class TagCommand : ICommand
{
    private readonly FileSystemNode _node;
    private readonly Tag _tag;
    private readonly bool _add;

    private TagCommand(FileSystemNode node, Tag tag, bool add)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
        _tag = tag;
        _add = add;
    }

    public static TagCommand Add(FileSystemNode node, Tag tag) => new(node, tag, add: true);

    public static TagCommand Remove(FileSystemNode node, Tag tag) => new(node, tag, add: false);

    public void Execute()
    {
        if (_add)
        {
            _node.AddTag(_tag);
        }
        else
        {
            _node.RemoveTag(_tag);
        }
    }

    public void Undo()
    {
        if (_add)
        {
            _node.RemoveTag(_tag);
        }
        else
        {
            _node.AddTag(_tag);
        }
    }
}
