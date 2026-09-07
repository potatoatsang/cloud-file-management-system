namespace FileSystem.Domain;

public static class SizeFormatter
{
    public const int BytesPerKilobyte = 1024;
    public const long BytesPerMegabyte = 1024L * 1024;

    public static string Format(long bytes)
    {
        if (bytes >= BytesPerMegabyte && bytes % BytesPerMegabyte == 0)
        {
            return $"{bytes / BytesPerMegabyte}MB";
        }

        if (bytes >= BytesPerKilobyte && bytes % BytesPerKilobyte == 0)
        {
            return $"{bytes / BytesPerKilobyte}KB";
        }

        return $"{bytes}B";
    }
}
