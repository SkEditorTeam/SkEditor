using System;
using Newtonsoft.Json;

namespace SkEditor.Utilities.Docs.SkriptMC;

public class SkriptMcDocEntry : IDocumentationEntry
{
    [JsonProperty("example")] public string RawExample;

    public IDocumentationExample Example => new SkriptMCDocExample { Example = RawExample };

    [JsonProperty("category")] public string RawType { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("content")] public string Description { get; set; }

    [JsonProperty("pattern")] public string Patterns { get; set; }

    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("addon")] public string Addon { get; set; }

    [JsonProperty("version")] public string Version { get; set; }

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