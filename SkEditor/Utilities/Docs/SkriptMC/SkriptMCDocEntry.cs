using System;
using Newtonsoft.Json;

namespace SkEditor.Utilities.Docs.SkriptMC;

public class SkriptMcDocEntry : IDocumentationEntry
{
    [JsonProperty("example")] public required string RawExample;

    public IDocumentationExample Example => new SkriptMcDocExample { Example = RawExample };

    [JsonProperty("category")] public required string RawType { get; set; }

    [JsonProperty("name")] public required string Name { get; set; }

    [JsonProperty("content")] public required string Description { get; set; }

    [JsonProperty("pattern")] public required string Patterns { get; set; }

    [JsonProperty("id")] public required string Id { get; set; }

    [JsonProperty("addon")] public required string Addon { get; set; }

    [JsonProperty("version")] public required string Version { get; set; }

    public IDocumentationEntry.Type DocType
    {
        get => Enum.Parse<IDocumentationEntry.Type>(RawType[..^1], true);
        set => RawType = value.ToString().ToLower() + "s";
    }

    public DocProvider Provider => DocProvider.SkriptMC;

    public string? ReturnType { get; set; }
    public string? Changers { get; set; }
    public string? EventValues { get; set; }
}