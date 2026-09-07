namespace FileSystem.Domain.Sorting;

public sealed class SizeSortStrategy : SortStrategy
{
    protected override int Compare(FileSystemNode left, FileSystemNode right) =>
        left.CalculateSize().CompareTo(right.CalculateSize());
}
