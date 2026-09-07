namespace FileSystem.Domain.Commands;

public sealed class SortCommand : ICommand
{
    private readonly Directory _directory;
    private readonly ISortStrategy _strategy;
    private readonly SortDirection _direction;
    private IReadOnlyList<FileSystemNode>? _previousOrder;

    public SortCommand(Directory directory, ISortStrategy strategy, SortDirection direction)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        _direction = direction;
    }

    public void Execute()
    {
        _previousOrder = _directory.Children.ToList();
        _directory.Sort(_strategy, _direction);
    }

    public void Undo()
    {
        if (_previousOrder is null)
        {
            return;
        }

        _directory.RestoreChildrenOrder(_previousOrder);
        _previousOrder = null;
    }
}
