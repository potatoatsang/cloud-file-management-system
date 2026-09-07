using FileSystem.Domain;
using FileSystem.Domain.Visitors;
using FluentAssertions;
using Directory = FileSystem.Domain.Directory;

namespace FileSystem.Tests;

public sealed class EssentialFeaturesTests
{
    private readonly Directory _root = SampleTreeFactory.Create();

    [Fact]
    public void Sample_tree_structure_and_file_metadata_are_correct()
    {
        _root.Name.Should().Be("根目錄");
        _root.Parent.Should().BeNull();
        _root.EnglishName.Should().Be("Root");

        var projectDocs = FindDirectory(_root, "專案文件");
        var personalNotes = FindDirectory(_root, "個人筆記");
        var archive = personalNotes.Children.OfType<Directory>()
            .Should()
            .ContainSingle(d => d.DisplayName == "2025備份 (Archive_2025)")
            .Subject;
        archive.Name.Should().Be("2025備份");
        archive.EnglishName.Should().Be("Archive_2025");
        archive.XmlTag.Should().Be("Archive_2025");

        var spec = FindFile<WordFile>(projectDocs, "需求規格書.docx");
        spec.PageCount.Should().Be(15);
        spec.SizeBytes.Should().Be(500 * 1024);
        spec.Extension.Should().Be("docx");

        var architecture = FindFile<ImageFile>(projectDocs, "系統架構圖.png");
        architecture.Width.Should().Be(1920);
        architecture.Height.Should().Be(1080);
        architecture.SizeBytes.Should().Be(2L * 1024 * 1024);
        architecture.Extension.Should().Be("png");

        var todo = FindFile<TextFile>(personalNotes, "待辦清單.txt");
        todo.Encoding.Should().Be("UTF-8");
        todo.SizeBytes.Should().Be(1024);
        todo.Extension.Should().Be("txt");

        var oldMinutes = FindFile<WordFile>(archive, "舊會議記錄.docx");
        oldMinutes.PageCount.Should().Be(5);
        oldMinutes.SizeBytes.Should().Be(200 * 1024);
        oldMinutes.Extension.Should().Be("docx");

        var readme = FindFile<TextFile>(_root, "README.txt");
        readme.Encoding.Should().Be("ASCII");
        readme.SizeBytes.Should().Be(500);
        readme.Extension.Should().Be("txt");
        readme.Parent.Should().Be(_root);
    }

    [Fact]
    public void Root_total_size_uses_1024_based_units()
    {
        const long expectedBytes =
            500L * 1024
            + 2L * 1024 * 1024
            + 1024
            + 200L * 1024
            + 500;

        expectedBytes.Should().Be(2_815_476);

        var visitor = new CalculateSizeVisitor();
        _root.Accept(visitor);

        _root.CalculateSize().Should().Be(expectedBytes);
        visitor.TotalSize.Should().Be("2815476B");
    }

    [Fact]
    public void Any_directory_size_is_computed_by_visitor()
    {
        var projectDocs = FindDirectory(_root, "專案文件");
        var archive = FindDirectory(FindDirectory(_root, "個人筆記"), "2025備份");

        var projectVisitor = new CalculateSizeVisitor();
        projectDocs.Accept(projectVisitor);
        projectVisitor.TotalSize.Should().Be("2548KB");

        var archiveVisitor = new CalculateSizeVisitor();
        archive.Accept(archiveVisitor);
        archiveVisitor.TotalSize.Should().Be("200KB");
        archiveVisitor.Trail.Should().Contain("Visiting: 個人筆記 -> 2025備份");
        archiveVisitor.Trail.Should().Contain("Visiting: 2025備份 -> 舊會議記錄.docx");
    }

    [Fact]
    public void Search_docx_returns_exactly_two_full_paths()
    {
        var visitor = new SearchVisitor(".docx");
        _root.Accept(visitor);

        visitor.MatchedPaths.Should().HaveCount(2);
        visitor.MatchedPaths.Should().Contain(p => p.Contains("需求規格書"));
        visitor.MatchedPaths.Should().Contain(p => p.Contains("舊會議記錄"));
    }

    [Fact]
    public void Search_docx_without_dot_matches_the_same_files()
    {
        var dotted = new SearchVisitor(".docx");
        var plain = new SearchVisitor("docx");
        _root.Accept(dotted);
        _root.Accept(plain);

        plain.MatchedPaths.Should().BeEquivalentTo(dotted.MatchedPaths);
    }

    [Fact]
    public void SearchVisitor_accepts_any_extension_and_can_start_from_any_directory()
    {
        var png = new SearchVisitor(".png");
        _root.Accept(png);
        png.MatchedPaths.Should().ContainSingle()
            .Which.Should().Contain("系統架構圖");

        var txt = new SearchVisitor("txt");
        _root.Accept(txt);
        txt.MatchedPaths.Should().HaveCount(2);

        var projectDocs = FindDirectory(_root, "專案文件");
        var docxInProject = new SearchVisitor(".docx");
        projectDocs.Accept(docxInProject);
        docxInProject.MatchedPaths.Should().ContainSingle()
            .Which.Should().Contain("需求規格書");
        docxInProject.MatchedPaths.Should().NotContain(p => p.Contains("舊會議記錄"));
    }

    [Fact]
    public void Xml_export_contains_root_tag_and_sample_nodes()
    {
        var visitor = new XmlExportVisitor();
        _root.Accept(visitor);

        visitor.Xml.Should().Contain("<根目錄_Root>");
        visitor.Xml.Should().Contain("</根目錄_Root>");
        visitor.Xml.Should().Contain("<專案文件_Project_Docs>");
        visitor.Xml.Should().Contain("<需求規格書_docx>頁數: 15, 大小: 500KB</需求規格書_docx>");
        visitor.Xml.Should().Contain("<系統架構圖_png>解析度: 1920x1080, 大小: 2MB</系統架構圖_png>");
        visitor.Xml.Should().Contain("<個人筆記_Personal_Notes>");
        visitor.Xml.Should().Contain("<待辦清單_txt>編碼: UTF-8, 大小: 1KB</待辦清單_txt>");
        visitor.Xml.Should().Contain("<Archive_2025>");
        visitor.Xml.Should().Contain("<舊會議記錄_docx>頁數: 5, 大小: 200KB</舊會議記錄_docx>");
        visitor.Xml.Should().Contain("<README_txt>編碼: ASCII, 大小: 500B</README_txt>");
        visitor.Xml.Should().NotContain("<2025備份_Archive_2025>");
    }

    [Fact]
    public void Print_matches_sample_plaintext_branches_and_archive_display_name()
    {
        var visitor = new PrintVisitor();
        _root.Accept(visitor);

        var output = visitor.Output.ReplaceLineEndings("\n");
        output.Should().Contain("2025備份 (Archive_2025)");
        output.Should().Contain("├──");
        output.Should().Contain("└──");
        output.Should().Contain("[目錄]");
        output.Should().Contain("[子目錄]");

        const string expected = """
            根目錄 (Root)
            ├── 專案文件 (Project_Docs) [目錄]
            │   ├── 需求規格書.docx [Word 檔案] (頁數: 15, 大小: 500KB)
            │   └── 系統架構圖.png [圖片] (解析度: 1920x1080, 大小: 2MB)
            ├── 個人筆記 (Personal_Notes) [目錄]
            │   ├── 待辦清單.txt [純文字檔] (編碼: UTF-8, 大小: 1KB)
            │   └── 2025備份 (Archive_2025) [子目錄]
            │       └── 舊會議記錄.docx [Word 檔案] (頁數: 5, 大小: 200KB)
            └── README.txt [純文字檔] (編碼: ASCII, 大小: 500B)
            """;

        output.TrimEnd().Should().Be(expected.ReplaceLineEndings("\n").TrimEnd());
        visitor.Trail.Should().Contain(line => line.Contains("2025備份"));
    }

    [Fact]
    public void Size_and_search_trails_are_non_empty_and_include_visited_names()
    {
        var size = new CalculateSizeVisitor();
        _root.Accept(size);
        AssertTrail(size.Trail);

        var search = new SearchVisitor(".docx");
        _root.Accept(search);
        AssertTrail(search.Trail);
    }

    private static void AssertTrail(IReadOnlyList<string> trail)
    {
        trail.Should().NotBeEmpty();
        trail.Should().Contain(line => line.Contains("根目錄"));
        trail.Should().Contain(line => line.Contains("專案文件"));
        trail.Should().Contain(line => line.Contains("需求規格書.docx"));
        trail.Should().Contain(line => line.Contains("個人筆記"));
        trail.Should().Contain(line => line.Contains("2025備份"));
        trail.Should().Contain(line => line.Contains("舊會議記錄.docx"));
        trail.Should().OnlyContain(line => line.StartsWith("Visiting: "));
        trail.Should().Contain("Visiting: (root) -> 根目錄");
        trail.Should().Contain("Visiting: 專案文件 -> 需求規格書.docx");
        trail.Should().OnlyContain(line => line.Contains(" -> "));
    }

    private static Directory FindDirectory(Directory parent, string name) =>
        parent.Children.OfType<Directory>().Should().ContainSingle(d => d.Name == name).Subject;

    private static TFile FindFile<TFile>(Directory parent, string name) where TFile : global::FileSystem.Domain.File =>
        parent.Children.OfType<TFile>().Should().ContainSingle(f => f.Name == name).Subject;
}
