namespace SkEditor.Models.UI;

public class SyntaxPairDto(string[] extensions, string? grammar, string theme)
{
    public string[] Extensions { get; set; } = extensions;
    public string? Grammar { get; set; } = grammar;
    public string Theme { get; set; } = theme;
}