using SkEditor.Matcher.Registries;

namespace SkEditor.Matcher.Models;

public class SyntaxElement
{
    public required string[] Patterns { get; set; }
    public required SyntaxElementDto.SyntaxType Type { get; set; }
    public TypeToken? ReturnType { get; set; }
    public string? Name { get; set; }

    public static SyntaxElement FromDto(SyntaxElementDto dto, TypeRegistry typeRegistry)
    {
        return new SyntaxElement
        {
            Patterns = dto.Patterns,
            Type = dto.Type,
            ReturnType = dto.ReturnType != null && typeRegistry.TryParseToken(dto.ReturnType, out TypeToken token)
                ? token
                : null,
            Name = dto.Name
        };
    }
}