namespace FileSystem.Domain.Commands;

public sealed class CopyPasteNodeCommand : ICommand
{
    private readonly FileSystemNode _source;
    private readonly Directory _target;
    private FileSystemNode? _pasted;

    public CopyPasteNodeCommand(FileSystemNode source, Directory target)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _target = target ?? throw new ArgumentNullException(nameof(target));
    }

    public void Execute()
    {
        if (_pasted is not null)
        {
            throw new InvalidOperationException("Command has already been executed.");
        }

        _pasted = _source.Clone();
        _target.Add(_pasted);
    }

    public void Undo()
    {
        if (_pasted is null)
        {
            return;
        }

        _target.Remove(_pasted);
        _pasted = null;
    }
}
