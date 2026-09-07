using System.Text;

namespace FileSystem.Domain.Visitors;

public sealed class XmlExportVisitor : FsVisitor
{
    private readonly StringBuilder _xml = new();
    private int _depth;

    public string Xml => _xml.ToString();

    public override void VisitDirectory(Directory directory)
    {
        Record(directory);
        var tag = directory.XmlTag;
        AppendLine($"<{tag}>");
        _depth++;
        VisitChildren(directory);
        _depth--;
        AppendLine($"</{tag}>");
    }

    public override void VisitWordFile(WordFile file) =>
        WriteFile(file, $"頁數: {file.PageCount}, 大小: {SizeFormatter.Format(file.SizeBytes)}");

    public override void VisitImageFile(ImageFile file) =>
        WriteFile(file, $"解析度: {file.Width}x{file.Height}, 大小: {SizeFormatter.Format(file.SizeBytes)}");

    public override void VisitTextFile(TextFile file) =>
        WriteFile(file, $"編碼: {file.Encoding}, 大小: {SizeFormatter.Format(file.SizeBytes)}");

    private void WriteFile(File file, string content)
    {
        Record(file);
        AppendLine($"<{file.XmlTag}>{content}</{file.XmlTag}>");
    }

    private void AppendLine(string line)
    {
        _xml.Append(' ', _depth * 4);
        _xml.AppendLine(line);
    }
}
