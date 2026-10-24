using System;
using SkEditor.Core.Localization;

namespace SkEditor.Core.Extensions;

public static class TextExtensions
{
    extension(string text)
    {
        public string NormalizePathSeparators()
        {
            return text.Replace("\\", "/");
        }

        public TranslatableString ToTranslatable()
        {
            return TranslatableString.FromRawString(text);
        }

        /// <summary>
        ///     Only use this for quickly creating hard-coded translatable strings in development.
        ///     The obsolete attribute will help you to find usages quickly.
        /// </summary>
        [Obsolete("Replace with proper translations keys later")]
        public TranslatableString ToTranslatableWip()
        {
            return TranslatableString.FromRawString(text);
        }
    }

    extension(ReadOnlySpan<char> source)
    {
        public void ToTitleCase(Span<char> destination)
        {
            if (source.Length > destination.Length)
            {
                throw new ArgumentException("Destination span is too short.");
            }

            bool isNewWord = true;
            for (int i = 0; i < source.Length; i++)
            {
                char c = source[i];

                if (char.IsLetter(c))
                {
                    destination[i] = isNewWord ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c);
                    isNewWord = false;
                }
                else
                {
                    destination[i] = c;

                    if (c != '\'')
                    {
                        isNewWord = true;
                    }
                }
            }
        }
    }
}