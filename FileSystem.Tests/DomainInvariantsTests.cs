using FileSystem.Domain;
using FileSystem.Domain.Visitors;
using FluentAssertions;
using Directory = FileSystem.Domain.Directory;

namespace FileSystem.Tests;

public sealed class DomainInvariantsTests
{
    [Fact]
    public void Root_parent_is_null_and_file_in_tree_must_belong_to_directory()
    {
        var root = new Directory("根");
        var loose = new TextFile("note.txt", 10, "UTF-8");
        loose.Parent.Should().BeNull();

        root.Add(loose);

        root.Parent.Should().BeNull();
        loose.Parent.Should().Be(root);
        loose.Parent.Should().BeOfType<Directory>();
    }

    [Fact]
    public void File_has_no_children_collection()
    {
        typeof(global::FileSystem.Domain.File).GetProperty("Children").Should().BeNull();
        typeof(Directory).GetProperty("Children").Should().NotBeNull();
    }

    [Fact]
    public void Add_rejects_cycles_including_adding_self_or_an_ancestor()
    {
        var root = new Directory("根");
        var child = new Directory("子");
        root.Add(child);

        var addSelf = () => root.Add(root);
        addSelf.Should().Throw<InvalidOperationException>();

        var addAncestor = () => child.Add(root);
        addAncestor.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PrintVisitor_walks_the_given_tree_and_is_not_a_hardcoded_sample()
    {
        var root = new Directory("自訂根", "Custom");
        var folder = new Directory("僅此資料夾", "OnlyFolder");
        folder.Add(new TextFile("獨立檔.txt", 500, "UTF-8"));
        root.Add(folder);
        root.Add(new TextFile("另一檔.txt", 10, "ASCII"));

        var visitor = new PrintVisitor();
        root.Accept(visitor);

        var output = visitor.Output;
        output.Should().Contain("自訂根 (Custom)");
        output.Should().Contain("├──");
        output.Should().Contain("└──");
        output.Should().Contain("僅此資料夾 (OnlyFolder) [目錄]");
        output.Should().Contain("獨立檔.txt");
        output.Should().NotContain("需求規格書");
        output.Should().NotContain("2025備份");
    }
}
