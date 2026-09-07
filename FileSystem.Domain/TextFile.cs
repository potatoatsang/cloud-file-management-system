namespace FileSystem.Domain;

public sealed class TextFile : File
{
    public TextFile(string name, long sizeBytes, string encoding)
        : base(name, sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(encoding))
        {
            throw new ArgumentException("Encoding is required.", nameof(encoding));
        }

        Encoding = encoding;
    }

    public string Encoding { get; }

    public override void Accept(IFsVisitor visitor) => visitor.VisitTextFile(this);
}
