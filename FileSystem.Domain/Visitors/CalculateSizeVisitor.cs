namespace FileSystem.Domain.Visitors;

public sealed class CalculateSizeVisitor : FsVisitor
{
    private long _totalBytes;

    public string TotalSize => SizeFormatter.Format(_totalBytes);

    public override void VisitDirectory(Directory directory)
    {
        Record(directory);
        VisitChildren(directory);
    }

    public override void VisitWordFile(WordFile file) => AddFile(file);

    public override void VisitImageFile(ImageFile file) => AddFile(file);

    public override void VisitTextFile(TextFile file) => AddFile(file);

    private void AddFile(File file)
    {
        Record(file);
        _totalBytes += file.SizeBytes;
    }
}
