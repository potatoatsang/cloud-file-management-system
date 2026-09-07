namespace FileSystem.Domain.Sorting;

public abstract class SortStrategy : ISortStrategy
{
    public void Sort(Directory directory, SortDirection direction)
    {
        ArgumentNullException.ThrowIfNull(directory);
        directory.SortInPlace((left, right) => Apply(Compare(left, right), direction));
    }

    protected abstract int Compare(FileSystemNode left, FileSystemNode right);

    private static int Apply(int comparison, SortDirection direction) =>
        direction == SortDirection.Descending ? -comparison : comparison;
}
