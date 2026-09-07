using FileSystem.Domain;
using FluentAssertions;
using Directory = FileSystem.Domain.Directory;

namespace FileSystem.Tests;

public sealed class TaggingTests
{
    [Fact]
    public void Directory_and_file_support_multiple_tags_and_removal()
    {
        var folder = new Directory("專案文件", "Project_Docs");
        var file = new TextFile("README.txt", 500, "ASCII");

        folder.AddTag(Tag.Work);
        folder.AddTag(Tag.Urgent);
        folder.AddTag(Tag.Work);

        folder.HasTag(Tag.Work).Should().BeTrue();
        folder.HasTag(Tag.Urgent).Should().BeTrue();
        folder.HasTag(Tag.Personal).Should().BeFalse();
        folder.Tags.Should().BeEquivalentTo([Tag.Work, Tag.Urgent]);

        folder.RemoveTag(Tag.Urgent);
        folder.HasTag(Tag.Urgent).Should().BeFalse();
        folder.Tags.Should().Equal(Tag.Work);

        file.AddTag(Tag.Personal);
        file.HasTag(Tag.Personal).Should().BeTrue();
        file.Tags.Should().ContainSingle().Which.Should().Be(Tag.Personal);
    }

    [Fact]
    public void Tags_have_distinct_domain_colors()
    {
        TagPalette.ColorName(Tag.Urgent).Should().Be("紅");
        TagPalette.ColorName(Tag.Work).Should().Be("藍");
        TagPalette.ColorName(Tag.Personal).Should().Be("綠");
    }

    [Fact]
    public void Clone_copies_tags_onto_the_new_node()
    {
        var file = new WordFile("需求規格書.docx", 500 * 1024, 15);
        file.AddTag(Tag.Work);
        file.AddTag(Tag.Urgent);

        var clone = file.Clone();
        clone.Id.Should().NotBe(file.Id);
        clone.Tags.Should().BeEquivalentTo([Tag.Work, Tag.Urgent]);
    }
}
