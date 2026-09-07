namespace FileSystem.Domain;

public sealed class ImageFile : File
{
    public ImageFile(string name, long sizeBytes, int width, int height)
        : base(name, sizeBytes)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        Width = width;
        Height = height;
    }

    public int Width { get; }

    public int Height { get; }

    public override void Accept(IFsVisitor visitor) => visitor.VisitImageFile(this);

    public override FileSystemNode Clone() => CopyTagsTo(new ImageFile(Name, SizeBytes, Width, Height));
}
