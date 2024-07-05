namespace SkEditor.Utilities;
public static class StringExtensions
{
    public static string FixLinuxPath(this string path)
    {
        return path.Replace("\\", "/");
    }
}