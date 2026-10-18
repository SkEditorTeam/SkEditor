using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkEditor.Analyzing.Models;

[JsonSerializable(typeof(AnalyzeResultsDto))]
public partial class AnalyzeResultContext : JsonSerializerContext;

public class AnalyzeResultsDto
{
    [JsonPropertyName("results")]
    public required Dictionary<string, AnalyzeResultDto> Results { get; set; }
}

public class AnalyzeResultDto
{
    [JsonPropertyName("errors")]
    public required List<ScriptErrorDto> Errors { get; set; }

    [JsonPropertyName("structure")]
    public required ScriptStructureDto Structure { get; set; }

    [JsonPropertyName("usedAddons")]
    public required HashSet<string> UsedAddons { get; set; }
}

public class ScriptErrorDto
{
    [JsonConverter(typeof(JsonStringEnumConverter<AnalyzerErrorLevel>))]
    public enum AnalyzerErrorLevel
    {
        Severe,
        Warning,
        Info
    }

    [JsonPropertyName("level")]
    public required AnalyzerErrorLevel Level { get; set; }

    [JsonPropertyName("line")]
    public required int Line { get; set; }

    [JsonPropertyName("message")]
    public required string Message { get; set; }
}

public class ScriptStructureDto
{
    [JsonPropertyName("commands")]
    public required List<CommandDataDto> Commands { get; set; }

    [JsonPropertyName("events")]
    public required List<EventDataDto> Events { get; set; }

    [JsonPropertyName("functions")]
    public required List<FunctionDataDto> Functions { get; set; }

    [JsonPropertyName("options")]
    public required Dictionary<string, string> Options { get; set; }
}

public record CommandDataDto(
    [property: JsonPropertyName("line")]
    int Line,
    [property: JsonPropertyName("value")]
    string Value,
    [property: JsonPropertyName("aliases")]
    List<string> Aliases,
    [property: JsonPropertyName("permission")]
    string? Permission,
    [property: JsonPropertyName("description")]
    string? Description,
    [property: JsonPropertyName("prefix")]
    string Prefix,
    [property: JsonPropertyName("usage")]
    string? Usage,
    [property: JsonPropertyName("arguments")]
    List<string> Arguments
);

public record EventDataDto(
    [property: JsonPropertyName("line")]
    int Line,
    [property: JsonPropertyName("value")]
    string Value,
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("eventPriority")]
    string EventPriority
);

public record FunctionDataDto(
    [property: JsonPropertyName("line")]
    int Line,
    [property: JsonPropertyName("value")]
    string Value,
    [property: JsonPropertyName("local")]
    bool IsLocal,
    [property: JsonPropertyName("parameters")]
    Dictionary<string, string> Parameters,
    [property: JsonPropertyName("returnType")]
    string? ReturnType
);