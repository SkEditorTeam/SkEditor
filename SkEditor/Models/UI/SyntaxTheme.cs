using System;

namespace SkEditor.Models.UI;

public record SyntaxTheme(string FileName, string? Name, string? TargetLanguage = null)
{
    public bool IsUniversal => string.IsNullOrEmpty(TargetLanguage) || TargetLanguage == "*";
    public bool IsBuiltInTextMateTheme { get; set; }

    public bool IsForLanguage(string language)
    {
        return IsUniversal || string.Equals(TargetLanguage, language, StringComparison.OrdinalIgnoreCase);
    }
}