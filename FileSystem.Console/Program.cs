using FileSystem.Domain;
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
