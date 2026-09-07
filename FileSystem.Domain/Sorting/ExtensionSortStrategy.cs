namespace FileSystem.Domain.Sorting;

public sealed class ExtensionSortStrategy : SortStrategy
{
    protected override int Compare(FileSystemNode left, FileSystemNode right) =>
        string.Compare(ExtensionOf(left), ExtensionOf(right), StringComparison.OrdinalIgnoreCase);

    private static string ExtensionOf(FileSystemNode node) =>
        node is File file ? file.Extension : string.Empty;
}
