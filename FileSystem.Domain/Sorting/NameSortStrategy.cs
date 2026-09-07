namespace FileSystem.Domain.Sorting;

public sealed class NameSortStrategy : SortStrategy
{
    protected override int Compare(FileSystemNode left, FileSystemNode right) =>
        string.Compare(left.Name, right.Name, StringComparison.Ordinal);
}
