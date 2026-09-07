namespace FileSystem.Domain;

public interface ISortStrategy
{
    void Sort(Directory directory, SortDirection direction);
}
