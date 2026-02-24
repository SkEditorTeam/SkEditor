using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkEditor.Utilities.Docs.SkriptMC;

public class SkriptMcDocEntry : IDocumentationEntry
{
    [JsonPropertyName("example")]
    public required string RawExample;

    public IDocumentationExample Example => new SkriptMcDocExample { Example = RawExample };

    [JsonPropertyName("category")]
    public required string RawType { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("content")]
    public required string Description { get; set; }

    [JsonPropertyName("pattern")]
    public required string Patterns { get; set; }

    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("addon")]
    public required string Addon { get; set; }

    [JsonPropertyName("version")]
    public required string Version { get; set; }

    public IDocumentationEntry.Type DocType
    {
        get => Enum.Parse<IDocumentationEntry.Type>(RawType[..^1], true);
        set => RawType = value.ToString().ToLower() + "s";
    }

    public DocProvider Provider => DocProvider.SkriptMc;

    public string? ReturnType { get; set; }
    public string? Changers { get; set; }
    public string? EventValues { get; set; }
}

[JsonSerializable(typeof(SkriptMcDocEntry))]
[JsonSerializable(typeof(List<SkriptMcDocEntry>))]
public partial class SkriptMcDocJsonContext : JsonSerializerContext;