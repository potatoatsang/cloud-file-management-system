namespace FileSystem.Domain.Commands;

public sealed class DeleteNodeCommand : ICommand
{
    private readonly FileSystemNode _node;
    private Directory? _parent;
    private int _index = -1;

    public DeleteNodeCommand(FileSystemNode node)
    {
        _node = node ?? throw new ArgumentNullException(nameof(node));
    }

    public void Execute()
    {
        if (_node.Parent is null)
        {
            throw new InvalidOperationException("Cannot delete the root directory.");
        }

        _parent = _node.Parent;
        _index = _parent.IndexOf(_node);
        if (!_parent.Remove(_node))
        {
            throw new InvalidOperationException("Node is not a child of its parent.");
        }
    }

    public void Undo()
    {
        if (_parent is null || _index < 0)
        {
            return;
        }

        _parent.Insert(_index, _node);
        _parent = null;
        _index = -1;
    }
}
