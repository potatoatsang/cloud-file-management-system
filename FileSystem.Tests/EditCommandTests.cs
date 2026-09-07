using FileSystem.Domain;
using FileSystem.Domain.Commands;
using FileSystem.Domain.Visitors;
using FluentAssertions;
using Directory = FileSystem.Domain.Directory;

namespace FileSystem.Tests;

public sealed class EditCommandTests
{
    [Fact]
    public void Remove_detaches_child_and_insert_restores_index()
    {
        var folder = new Directory("資料夾");
        var first = new TextFile("a.txt", 1, "UTF-8");
        var second = new TextFile("b.txt", 1, "UTF-8");
        folder.Add(first);
        folder.Add(second);

        folder.Remove(first).Should().BeTrue();
        first.Parent.Should().BeNull();
        folder.Children.Should().Equal(second);

        folder.Insert(0, first);
        folder.Children.Should().Equal(first, second);
        first.Parent.Should().Be(folder);
    }

    [Fact]
    public void DeleteNodeCommand_rejects_root_and_undo_restores_original_index()
    {
        var root = new Directory("根");
        var a = new TextFile("a.txt", 1, "UTF-8");
        var b = new TextFile("b.txt", 1, "UTF-8");
        var c = new TextFile("c.txt", 1, "UTF-8");
        root.Add(a);
        root.Add(b);
        root.Add(c);

        var deleteRoot = new DeleteNodeCommand(root);
        var act = () => deleteRoot.Execute();
        act.Should().Throw<InvalidOperationException>();

        var deleteB = new DeleteNodeCommand(b);
        deleteB.Execute();
        root.Children.Should().Equal(a, c);
        b.Parent.Should().BeNull();

        deleteB.Undo();
        root.Children.Should().Equal(a, b, c);
        b.Parent.Should().Be(root);
    }

    [Fact]
    public void Clone_deep_copies_subtree_with_new_ids()
    {
        var root = SampleTreeFactory.Create();
        var notes = root.Children.OfType<Directory>().Single(d => d.Name == "個人筆記");
        var clone = (Directory)notes.Clone();

        clone.Should().NotBeSameAs(notes);
        clone.Id.Should().NotBe(notes.Id);
        clone.Parent.Should().BeNull();
        clone.Name.Should().Be("個人筆記");
        clone.Children.Should().HaveCount(notes.Children.Count);

        var originalArchive = notes.Children.OfType<Directory>().Single(d => d.Name == "2025備份");
        var clonedArchive = clone.Children.OfType<Directory>().Single(d => d.Name == "2025備份");
        clonedArchive.Id.Should().NotBe(originalArchive.Id);
        clonedArchive.XmlTag.Should().Be("Archive_2025");
        clonedArchive.Children.Single().Id.Should().NotBe(originalArchive.Children.Single().Id);

        notes.Children.Should().HaveCount(2);
    }

    [Fact]
    public void CopyPasteNodeCommand_pastes_clone_and_undo_removes_only_the_copy()
    {
        var root = SampleTreeFactory.Create();
        var project = root.Children.OfType<Directory>().Single(d => d.Name == "專案文件");
        var notes = root.Children.OfType<Directory>().Single(d => d.Name == "個人筆記");
        var spec = project.Children.OfType<WordFile>().Single(f => f.Name == "需求規格書.docx");

        var paste = new CopyPasteNodeCommand(spec, notes);
        paste.Execute();

        notes.Children.OfType<WordFile>().Should().ContainSingle(f => f.Name == "需求規格書.docx");
        var copy = notes.Children.OfType<WordFile>().Single(f => f.Name == "需求規格書.docx");
        copy.Id.Should().NotBe(spec.Id);
        copy.Parent.Should().Be(notes);
        spec.Parent.Should().Be(project);

        paste.Undo();
        notes.Children.OfType<WordFile>().Should().BeEmpty();
        spec.Parent.Should().Be(project);
    }

    [Fact]
    public void Delete_then_size_visitor_still_uses_total_size_contract()
    {
        var root = SampleTreeFactory.Create();
        var readme = root.Children.OfType<TextFile>().Single(f => f.Name == "README.txt");
        new DeleteNodeCommand(readme).Execute();

        var visitor = new CalculateSizeVisitor();
        root.Accept(visitor);
        visitor.TotalSize.Should().Be("2749KB");
    }
}
