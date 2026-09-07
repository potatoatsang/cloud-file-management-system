using FileSystem.Domain;
using FileSystem.Domain.Sorting;
using FileSystem.Domain.Visitors;
using FluentAssertions;
using Directory = FileSystem.Domain.Directory;

namespace FileSystem.Tests;

public sealed class SortStrategyTests
{
    [Fact]
    public void Name_strategy_sorts_children_ascending_and_descending()
    {
        var folder = MixedFolder();

        folder.Sort(new NameSortStrategy(), SortDirection.Ascending);
        folder.Children.Select(c => c.Name).Should().Equal("a.txt", "b.png", "c.docx");

        folder.Sort(new NameSortStrategy(), SortDirection.Descending);
        folder.Children.Select(c => c.Name).Should().Equal("c.docx", "b.png", "a.txt");
    }

    [Fact]
    public void Size_strategy_sorts_by_subtree_bytes()
    {
        var folder = MixedFolder();

        folder.Sort(new SizeSortStrategy(), SortDirection.Ascending);
        folder.Children.Select(c => c.Name).Should().Equal("a.txt", "c.docx", "b.png");

        folder.Sort(new SizeSortStrategy(), SortDirection.Descending);
        folder.Children.Select(c => c.Name).Should().Equal("b.png", "c.docx", "a.txt");
    }

    [Fact]
    public void Extension_strategy_sorts_files_by_extension()
    {
        var folder = MixedFolder();

        folder.Sort(new ExtensionSortStrategy(), SortDirection.Ascending);
        folder.Children.Select(c => c.Name).Should().Equal("c.docx", "b.png", "a.txt");

        folder.Sort(new ExtensionSortStrategy(), SortDirection.Descending);
        folder.Children.Select(c => c.Name).Should().Equal("a.txt", "b.png", "c.docx");
    }

    [Fact]
    public void Sorting_only_reorders_that_directory_and_keeps_parent_links()
    {
        var root = SampleTreeFactory.Create();
        var projectDocs = root.Children.OfType<Directory>().Single(d => d.Name == "專案文件");
        var beforeRootNames = root.Children.Select(c => c.Name).ToList();

        projectDocs.Sort(new NameSortStrategy(), SortDirection.Ascending);

        root.Children.Select(c => c.Name).Should().Equal(beforeRootNames);
        projectDocs.Children.Should().OnlyContain(c => ReferenceEquals(c.Parent, projectDocs));
        projectDocs.Children.Select(c => c.Name).Should().Equal("系統架構圖.png", "需求規格書.docx");
    }

    [Fact]
    public void PrintVisitor_reflects_sorted_children_order()
    {
        var folder = MixedFolder();
        folder.Sort(new NameSortStrategy(), SortDirection.Ascending);

        var print = new PrintVisitor();
        folder.Accept(print);

        var namesInOutput = new[] { "a.txt", "b.png", "c.docx" }
            .Select(n => print.Output.IndexOf(n, StringComparison.Ordinal))
            .ToList();
        namesInOutput.Should().BeInAscendingOrder();
    }

    [Fact]
    public void CalculateSizeVisitor_total_size_is_unchanged_after_sort()
    {
        var root = SampleTreeFactory.Create();
        var projectDocs = root.Children.OfType<Directory>().Single(d => d.Name == "專案文件");

        projectDocs.Sort(new SizeSortStrategy(), SortDirection.Descending);

        var visitor = new CalculateSizeVisitor();
        root.Accept(visitor);
        visitor.TotalSize.Should().Be("2815476B");
    }

    private static Directory MixedFolder()
    {
        var folder = new Directory("資料夾");
        folder.Add(new TextFile("a.txt", 10, "UTF-8"));
        folder.Add(new ImageFile("b.png", 30, 1, 1));
        folder.Add(new WordFile("c.docx", 20, 1));
        return folder;
    }
}
