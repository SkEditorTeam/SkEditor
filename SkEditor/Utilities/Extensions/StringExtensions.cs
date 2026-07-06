using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Extensions;

public static class StringExtensions
{
    extension(string path)
    {
        public string NormalizePathSeparators()
        {
            return path.Replace("\\", "/");
        }

        public TranslatableString ToTranslatable()
        {
            return TranslatableString.FromRawString(path);
        }
    }
}