using System.Text.Json.Serialization;

namespace SkEditor.Matcher.Models;

[JsonSerializable(typeof(SkriptDataDto))]
[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class SkriptDataJsonContext : JsonSerializerContext;

public class SkriptDataDto
{
    [JsonPropertyName("classInfos")]
    public required List<ClassInfoDto> ClassInfos { get; set; }

    [JsonPropertyName("globalFunctions")]
    public required List<FunctionDto> GlobalFunctions { get; set; }

    [JsonPropertyName("syntaxes")]
    public required List<SyntaxElementDto> SyntaxElements { get; set; }
}

public class ClassInfoDto
{
    [JsonPropertyName("codeName")]
    public required string CodeName { get; set; }

    [JsonPropertyName("plural")]
    public required string Plural { get; set; }

    [JsonPropertyName("parents")]
    public required string[] Parents { get; set; }

    [JsonPropertyName("regexPatterns")]
    public string[]? RegexPatterns { get; set; }
}

public class FunctionDto
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("parameters")]
    public required ClassTypeDto[] Parameters { get; set; }

    [JsonPropertyName("returnType")]
    public required string ReturnType { get; set; }
}

public class ClassTypeDto
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("isList")]
    public bool IsList { get; set; }
}

public class SyntaxElementDto
{
    [JsonConverter(typeof(JsonStringEnumConverter<SyntaxType>))]
    public enum SyntaxType
    {
        Structure,
        Section,
        Event,
        Expression,
        Condition,
        Effect
    }

    [JsonPropertyName("patterns")]
    public required string[] Patterns { get; set; }

    [JsonPropertyName("type")]
    public required SyntaxType Type { get; set; }

    [JsonPropertyName("returnType")]
    public string? ReturnType { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}