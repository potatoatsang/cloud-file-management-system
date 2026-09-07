using FileSystem.Domain;
using FileSystem.Domain.Commands;
using FileSystem.Domain.Sorting;
using FileSystem.Domain.Visitors;
using FluentAssertions;
using Directory = FileSystem.Domain.Directory;

namespace FileSystem.Tests;

public sealed class CommandHistoryTests
{
    [Fact]
    public void Empty_history_undo_and_redo_are_no_ops()
    {
        var history = new CommandHistory();
        history.CanUndo.Should().BeFalse();
        history.CanRedo.Should().BeFalse();

        var actUndo = () => history.Undo();
        var actRedo = () => history.Redo();
        actUndo.Should().NotThrow();
        actRedo.Should().NotThrow();
    }

    [Fact]
    public void History_undo_and_redo_delete_command()
    {
        var root = SampleTreeFactory.Create();
        var readme = root.Children.OfType<TextFile>().Single(f => f.Name == "README.txt");
        var history = new CommandHistory();

        history.Execute(new DeleteNodeCommand(readme));
        root.Children.Should().NotContain(readme);
        history.CanUndo.Should().BeTrue();
        history.CanRedo.Should().BeFalse();

        history.Undo();
        root.Children.Should().Contain(readme);
        readme.Parent.Should().Be(root);
        history.CanUndo.Should().BeFalse();
        history.CanRedo.Should().BeTrue();

        history.Redo();
        root.Children.Should().NotContain(readme);
        history.CanUndo.Should().BeTrue();
        history.CanRedo.Should().BeFalse();
    }

    [Fact]
    public void History_undo_and_redo_copy_paste_command()
    {
        var root = SampleTreeFactory.Create();
        var project = root.Children.OfType<Directory>().Single(d => d.Name == "專案文件");
        var notes = root.Children.OfType<Directory>().Single(d => d.Name == "個人筆記");
        var spec = project.Children.OfType<WordFile>().Single(f => f.Name == "需求規格書.docx");
        var history = new CommandHistory();

        history.Execute(new CopyPasteNodeCommand(spec, notes));
        notes.Children.OfType<WordFile>().Should().ContainSingle(f => f.Name == "需求規格書.docx");

        history.Undo();
        notes.Children.OfType<WordFile>().Should().BeEmpty();

        history.Redo();
        notes.Children.OfType<WordFile>().Should().ContainSingle(f => f.Name == "需求規格書.docx");
    }

    [Fact]
    public void SortCommand_undo_restores_original_children_order()
    {
        var folder = new Directory("資料夾");
        var a = new TextFile("a.txt", 10, "UTF-8");
        var b = new ImageFile("b.png", 30, 1, 1);
        var c = new WordFile("c.docx", 20, 1);
        folder.Add(a);
        folder.Add(b);
        folder.Add(c);

        var history = new CommandHistory();
        history.Execute(new SortCommand(folder, new NameSortStrategy(), SortDirection.Descending));
        folder.Children.Select(n => n.Name).Should().Equal("c.docx", "b.png", "a.txt");

        history.Undo();
        folder.Children.Should().Equal(a, b, c);

        history.Redo();
        folder.Children.Select(n => n.Name).Should().Equal("c.docx", "b.png", "a.txt");
    }

    [Fact]
    public void TagCommand_add_and_remove_are_reversible_via_history()
    {
        var file = new TextFile("README.txt", 500, "ASCII");
        var history = new CommandHistory();

        history.Execute(TagCommand.Add(file, Tag.Urgent));
        file.HasTag(Tag.Urgent).Should().BeTrue();

        history.Undo();
        file.HasTag(Tag.Urgent).Should().BeFalse();

        history.Redo();
        file.HasTag(Tag.Urgent).Should().BeTrue();

        history.Execute(TagCommand.Remove(file, Tag.Urgent));
        file.HasTag(Tag.Urgent).Should().BeFalse();

        history.Undo();
        file.HasTag(Tag.Urgent).Should().BeTrue();
    }

    [Fact]
    public void New_execute_clears_redo_stack()
    {
        var root = SampleTreeFactory.Create();
        var readme = root.Children.OfType<TextFile>().Single(f => f.Name == "README.txt");
        var project = root.Children.OfType<Directory>().Single(d => d.Name == "專案文件");
        var history = new CommandHistory();

        history.Execute(new DeleteNodeCommand(readme));
        history.Undo();
        history.CanRedo.Should().BeTrue();

        history.Execute(TagCommand.Add(project, Tag.Work));
        history.CanRedo.Should().BeFalse();
        project.HasTag(Tag.Work).Should().BeTrue();
        root.Children.Should().Contain(readme);
    }

    [Fact]
    public void TotalSize_contract_unchanged_after_history_delete_and_undo()
    {
        var root = SampleTreeFactory.Create();
        var readme = root.Children.OfType<TextFile>().Single(f => f.Name == "README.txt");
        var history = new CommandHistory();

        history.Execute(new DeleteNodeCommand(readme));
        var afterDelete = new CalculateSizeVisitor();
        root.Accept(afterDelete);
        afterDelete.TotalSize.Should().Be("2749KB");

        history.Undo();
        var afterUndo = new CalculateSizeVisitor();
        root.Accept(afterUndo);
        afterUndo.TotalSize.Should().Be("2815476B");
    }
}
