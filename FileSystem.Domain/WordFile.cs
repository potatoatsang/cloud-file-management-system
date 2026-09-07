namespace FileSystem.Domain;

public sealed class WordFile : File
{
    public WordFile(string name, long sizeBytes, int pageCount)
        : base(name, sizeBytes)
    {
        if (pageCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageCount));
        }

        PageCount = pageCount;
    }

    public int PageCount { get; }

    public override void Accept(IFsVisitor visitor) => visitor.VisitWordFile(this);
}
