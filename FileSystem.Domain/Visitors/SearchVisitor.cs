namespace FileSystem.Domain.Visitors;

public sealed class SearchVisitor : FsVisitor
{
    private readonly string _extension;
    private readonly List<string> _matchedPaths = [];

    public SearchVisitor(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            throw new ArgumentException("Extension is required.", nameof(extension));
        }

        _extension = Normalize(extension);
    }

    public IReadOnlyList<string> MatchedPaths => _matchedPaths;

    public override void VisitDirectory(Directory directory)
    {
        Record(directory);
        VisitChildren(directory);
    }

    public override void VisitWordFile(WordFile file) => MatchFile(file);

    public override void VisitImageFile(ImageFile file) => MatchFile(file);

    public override void VisitTextFile(TextFile file) => MatchFile(file);

    private void MatchFile(File file)
    {
        Record(file);
        if (string.Equals(Normalize(file.Extension), _extension, StringComparison.OrdinalIgnoreCase))
        {
            _matchedPaths.Add(file.GetFullPath());
        }
    }

    private static string Normalize(string extension) => extension.Trim().TrimStart('.');
}
