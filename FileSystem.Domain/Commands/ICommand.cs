namespace FileSystem.Domain.Commands;

public interface ICommand
{
    void Execute();

    void Undo();
}
