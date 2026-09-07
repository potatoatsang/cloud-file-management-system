using FileSystem.Domain;
using FileSystem.Domain.Commands;
using FileSystem.Domain.Sorting;
using FileSystem.Domain.Visitors;
using Directory = FileSystem.Domain.Directory;

namespace FileSystem.Cli;

public static class Program
{
    public static void Main()
    {
        var root = SampleTreeFactory.Create();

        WriteHeading("1) 目錄結構");
        var print = new PrintVisitor();
        root.Accept(print);
        System.Console.Write(print.Output);

        WriteHeading("2) Root 總容量");
        var size = new CalculateSizeVisitor();
        root.Accept(size);
        System.Console.WriteLine($"TotalSize: {size.TotalSize}");
        WriteTrail(size.Trail);

        var projectDocs = root.Children.OfType<Directory>()
            .Single(d => d.Name == "專案文件");
        var projectSize = new CalculateSizeVisitor();
        projectDocs.Accept(projectSize);
        System.Console.WriteLine();
        System.Console.WriteLine($"專案文件 TotalSize: {projectSize.TotalSize}");
        WriteTrail(projectSize.Trail);

        WriteHeading("3) 搜尋 .docx");
        var search = new SearchVisitor(".docx");
        root.Accept(search);
        foreach (var path in search.MatchedPaths)
        {
            System.Console.WriteLine(path);
        }

        System.Console.WriteLine();
        WriteTrail(search.Trail);

        var png = new SearchVisitor(".png");
        root.Accept(png);
        System.Console.WriteLine();
        System.Console.WriteLine("搜尋 .png:");
        foreach (var path in png.MatchedPaths)
        {
            System.Console.WriteLine(path);
        }

        System.Console.WriteLine();
        WriteTrail(png.Trail);

        WriteHeading("4) XML");
        var xml = new XmlExportVisitor();
        root.Accept(xml);
        System.Console.Write(xml.Xml);

        WriteHeading("5) 排序後印樹（專案文件）");
        PrintSorted(projectDocs, new NameSortStrategy(), SortDirection.Ascending, "名稱升冪");
        PrintSorted(projectDocs, new SizeSortStrategy(), SortDirection.Descending, "大小降冪");
        PrintSorted(projectDocs, new ExtensionSortStrategy(), SortDirection.Ascending, "副檔名升冪");

        WriteHeading("6) 刪除與複製貼上");
        var editRoot = SampleTreeFactory.Create();
        System.Console.WriteLine("--- 刪除前 ---");
        WriteTree(editRoot);

        var readme = editRoot.Children.OfType<TextFile>().Single(f => f.Name == "README.txt");
        new DeleteNodeCommand(readme).Execute();
        System.Console.WriteLine("--- 刪除 README.txt 後 ---");
        WriteTree(editRoot);

        var sourceProject = editRoot.Children.OfType<Directory>().Single(d => d.Name == "專案文件");
        var targetNotes = editRoot.Children.OfType<Directory>().Single(d => d.Name == "個人筆記");
        var spec = sourceProject.Children.OfType<WordFile>().Single(f => f.Name == "需求規格書.docx");
        new CopyPasteNodeCommand(spec, targetNotes).Execute();
        System.Console.WriteLine("--- 將需求規格書.docx 複製貼上至個人筆記後 ---");
        WriteTree(editRoot);

        WriteHeading("7) 貼標與移除");
        var tagRoot = SampleTreeFactory.Create();
        var taggedProject = tagRoot.Children.OfType<Directory>().Single(d => d.Name == "專案文件");
        var taggedSpec = taggedProject.Children.OfType<WordFile>().Single(f => f.Name == "需求規格書.docx");
        taggedProject.AddTag(Tag.Work);
        taggedSpec.AddTag(Tag.Urgent);
        taggedSpec.AddTag(Tag.Work);
        WriteNodeTags("專案文件（貼標後）", taggedProject);
        WriteNodeTags("需求規格書.docx（貼標後）", taggedSpec);

        taggedSpec.RemoveTag(Tag.Urgent);
        WriteNodeTags("需求規格書.docx（移除 Urgent 後）", taggedSpec);

        WriteHeading("8) Undo／Redo（CommandHistory）");
        var historyRoot = SampleTreeFactory.Create();
        var history = new CommandHistory();
        var historyReadme = historyRoot.Children.OfType<TextFile>().Single(f => f.Name == "README.txt");
        var historyProject = historyRoot.Children.OfType<Directory>().Single(d => d.Name == "專案文件");

        System.Console.WriteLine("--- 操作前 ---");
        WriteTree(historyRoot);
        WriteNodeTags("專案文件標籤", historyProject);

        history.Execute(new DeleteNodeCommand(historyReadme));
        history.Execute(TagCommand.Add(historyProject, Tag.Urgent));
        System.Console.WriteLine("--- Execute：刪除 README.txt + 專案文件貼 Urgent ---");
        WriteTree(historyRoot);
        WriteNodeTags("專案文件標籤", historyProject);
        System.Console.WriteLine($"CanUndo={history.CanUndo}, CanRedo={history.CanRedo}");
        System.Console.WriteLine();

        history.Undo();
        System.Console.WriteLine("--- Undo 一次（還原標籤） ---");
        WriteNodeTags("專案文件標籤", historyProject);

        history.Undo();
        System.Console.WriteLine("--- Undo 再一次（還原 README.txt） ---");
        WriteTree(historyRoot);
        System.Console.WriteLine($"CanUndo={history.CanUndo}, CanRedo={history.CanRedo}");
        System.Console.WriteLine();

        history.Redo();
        history.Redo();
        System.Console.WriteLine("--- Redo 兩次（再次刪除 README.txt + 貼 Urgent） ---");
        WriteTree(historyRoot);
        WriteNodeTags("專案文件標籤", historyProject);
        System.Console.WriteLine($"CanUndo={history.CanUndo}, CanRedo={history.CanRedo}");
    }

    private static void PrintSorted(
        Directory directory,
        ISortStrategy strategy,
        SortDirection direction,
        string caption)
    {
        directory.Sort(strategy, direction);
        var print = new PrintVisitor();
        directory.Accept(print);
        System.Console.WriteLine($"--- {caption} ---");
        System.Console.Write(print.Output);
        System.Console.WriteLine();
    }

    private static void WriteTree(Directory directory)
    {
        var print = new PrintVisitor();
        directory.Accept(print);
        System.Console.Write(print.Output);
        System.Console.WriteLine();
    }

    private static void WriteNodeTags(string caption, FileSystemNode node)
    {
        System.Console.WriteLine($"--- {caption} ---");
        if (node.Tags.Count == 0)
        {
            System.Console.WriteLine("(無標籤)");
            System.Console.WriteLine();
            return;
        }

        foreach (var tag in node.Tags)
        {
            System.Console.WriteLine($"{tag}（{TagPalette.ColorName(tag)}）");
        }

        System.Console.WriteLine();
    }

    private static void WriteHeading(string title)
    {
        System.Console.WriteLine();
        System.Console.WriteLine("======== " + title + " ========");
        System.Console.WriteLine();
    }

    private static void WriteTrail(IReadOnlyList<string> trail)
    {
        System.Console.WriteLine("Traverse Log:");
        foreach (var line in trail)
        {
            System.Console.WriteLine(line);
        }
    }
}
