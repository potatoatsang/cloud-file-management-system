namespace FileSystem.Domain;

public static class TagPalette
{
    public static string ColorName(Tag tag) => tag switch
    {
        Tag.Urgent => "紅",
        Tag.Work => "藍",
        Tag.Personal => "綠",
        _ => throw new ArgumentOutOfRangeException(nameof(tag), tag, "Unknown tag.")
    };
}
