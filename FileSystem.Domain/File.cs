namespace FileSystem.Domain;

public abstract class File : FileSystemNode
{
    protected File(string name, long sizeBytes)
        : base(name)
    {
        if (sizeBytes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeBytes));
        }

        SizeBytes = sizeBytes;
        Extension = ParseExtension(name);
    }

    public long SizeBytes { get; }

    public string Extension { get; }

    public string XmlTag
    {
        get
        {
            var stem = Name;
            var suffix = $".{Extension}";
            if (stem.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                stem = stem[..^suffix.Length];
            }

            return $"{stem}_{Extension}";
        }
    }

    public override long CalculateSize() => SizeBytes;

    private static string ParseExtension(string name)
    {
        var dot = name.LastIndexOf('.');
        if (dot < 0 || dot == name.Length - 1)
        {
            throw new ArgumentException("File name must include an extension.", nameof(name));
        }

        return name[(dot + 1)..];
    }
}
