namespace SkEditor.Models.Documentation;

public record DocumentationEntry
{
    public required string Addon { get; init; }
    public required string? Description { get; init; }
    public required string Name { get; init; }
    public required string Patterns { get; init; }
    public required DocumentationSyntaxType Type { get; init; }
    public required string? Version { get; init; }
}