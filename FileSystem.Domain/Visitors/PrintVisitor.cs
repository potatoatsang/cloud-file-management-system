using System.Text;

namespace FileSystem.Domain.Visitors;

public sealed class PrintVisitor : FsVisitor
{
    private readonly StringBuilder _output = new();
    private readonly List<bool> _isLastAtLevel = [];

    public string Output => _output.ToString();

    public override void VisitDirectory(Directory directory)
    {
        Record(directory);
        WriteLine(FormatDirectory(directory));
        VisitChildrenWithBranches(directory);
    }

    public override void VisitWordFile(WordFile file)
    {
        Record(file);
        WriteLine($"{file.Name} [Word 檔案] (頁數: {file.PageCount}, 大小: {SizeFormatter.Format(file.SizeBytes)})");
    }

    public override void VisitImageFile(ImageFile file)
    {
        Record(file);
        WriteLine($"{file.Name} [圖片] (解析度: {file.Width}x{file.Height}, 大小: {SizeFormatter.Format(file.SizeBytes)})");
    }

    public override void VisitTextFile(TextFile file)
    {
        Record(file);
        WriteLine($"{file.Name} [純文字檔] (編碼: {file.Encoding}, 大小: {SizeFormatter.Format(file.SizeBytes)})");
    }

    private void VisitChildrenWithBranches(Directory directory)
    {
        for (var i = 0; i < directory.Children.Count; i++)
        {
            _isLastAtLevel.Add(i == directory.Children.Count - 1);
            directory.Children[i].Accept(this);
            _isLastAtLevel.RemoveAt(_isLastAtLevel.Count - 1);
        }
    }

    private static string FormatDirectory(Directory directory)
    {
        if (directory.Parent is null)
        {
            return directory.DisplayName;
        }

        var kind = directory.Parent.Parent is null ? " [目錄]" : " [子目錄]";
        return directory.DisplayName + kind;
    }

    private void WriteLine(string text)
    {
        _output.Append(BuildPrefix());
        _output.AppendLine(text);
    }

    private string BuildPrefix()
    {
        if (_isLastAtLevel.Count == 0)
        {
            return string.Empty;
        }

        var prefix = new StringBuilder();
        for (var i = 0; i < _isLastAtLevel.Count - 1; i++)
        {
            prefix.Append(_isLastAtLevel[i] ? "    " : "│   ");
        }

        prefix.Append(_isLastAtLevel[^1] ? "└── " : "├── ");
        return prefix.ToString();
    }
}
