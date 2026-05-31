using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkEditor.Models.Documentation;

public record SkUnityResponseDto
{
    [JsonPropertyName("response")]
    public required string Response { get; init; }

    [JsonPropertyName("result")]
    public required SkUnityResultDto ResultDto { get; init; }
}

public record SkUnityResultDto
{
    [JsonPropertyName("docs")]
    public required Dictionary<string, SkUnityDataDto> Docs { get; init; }
}

public record SkUnityDataDto
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("doc")]
    public required string Type { get; init; }

    [JsonPropertyName("desc")]
    public required string Description { get; init; }

    [JsonPropertyName("addon")]
    public required string Addon { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("pattern")]
    public required string Pattern { get; init; }
}

[JsonSerializable(typeof(SkUnityResponseDto))]
[JsonSerializable(typeof(SkUnityResultDto))]
[JsonSerializable(typeof(SkUnityDataDto))]
[JsonSerializable(typeof(Dictionary<string, SkUnityDataDto>))]
public partial class SkUnityResponseContext : JsonSerializerContext;