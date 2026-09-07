namespace FileSystem.Domain;

public static class SampleTreeFactory
{
    public static Directory Create()
    {
        var root = new Directory("根目錄", "Root");
        var projectDocs = new Directory("專案文件", "Project_Docs");
        var personalNotes = new Directory("個人筆記", "Personal_Notes");
        var archive = new Directory("2025備份", "Archive_2025", xmlTag: "Archive_2025");

        projectDocs.Add(new WordFile("需求規格書.docx", 500L * SizeFormatter.BytesPerKilobyte, 15));
        projectDocs.Add(new ImageFile("系統架構圖.png", 2L * SizeFormatter.BytesPerMegabyte, 1920, 1080));

        archive.Add(new WordFile("舊會議記錄.docx", 200L * SizeFormatter.BytesPerKilobyte, 5));
        personalNotes.Add(new TextFile("待辦清單.txt", SizeFormatter.BytesPerKilobyte, "UTF-8"));
        personalNotes.Add(archive);

        root.Add(projectDocs);
        root.Add(personalNotes);
        root.Add(new TextFile("README.txt", 500, "ASCII"));

        return root;
    }
}
