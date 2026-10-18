using System;
using SkEditor.Core.Localization;

namespace SkEditor.Core.Extensions;

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

        /// <summary>
        ///     Only use this for quickly creating hard-coded translatable strings in development.
        ///     The obsolete attribute will help you to find usages quickly.
        /// </summary>
        [Obsolete("Replace with proper translations keys later")]
        public TranslatableString ToTranslatableWip()
        {
            return TranslatableString.FromRawString(path);
        }
    }
}