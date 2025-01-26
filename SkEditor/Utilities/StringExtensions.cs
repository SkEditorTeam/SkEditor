namespace SkEditor.Utilities;
public static class StringExtensions
{
    public static string NormalizePathSeparators(this string path)
    {
        return path.Replace("\\", "/");
    }
}