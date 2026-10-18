using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkEditor.Models.Documentation;

public record SkDocsResponseDto
{
    [JsonPropertyName("results")]
    public required List<SkDocsDataDto> Results { get; init; }
}

public record SkDocsDataDto
{
    [JsonPropertyName("addon")]
    public required string Addon { get; init; }

    [JsonPropertyName("category")]
    public required string Category { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("examples")]
    public string[]? Examples { get; init; }

    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("since")]
    public string? Since { get; init; }

    [JsonPropertyName("syntax")]
    public required string Syntax { get; init; }

    [JsonPropertyName("title")]
    public required string Title { get; init; }
}

[JsonSerializable(typeof(SkDocsResponseDto))]
[JsonSerializable(typeof(SkDocsDataDto))]
[JsonSerializable(typeof(List<SkDocsDataDto>))]
public partial class SkDocsResponseContext : JsonSerializerContext;