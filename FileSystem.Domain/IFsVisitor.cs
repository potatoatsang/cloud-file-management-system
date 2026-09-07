namespace FileSystem.Domain;

public interface IFsVisitor
{
    IReadOnlyList<string> Trail { get; }

    void VisitDirectory(Directory directory);

    void VisitWordFile(WordFile file);

    void VisitImageFile(ImageFile file);

    void VisitTextFile(TextFile file);
}
