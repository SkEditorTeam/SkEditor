using System;
using Newtonsoft.Json;

namespace SkEditor.Utilities.Docs.SkriptHub;

[Serializable]
public class SkriptHubDocExample : IDocumentationExample
{
    [JsonProperty("example_code")] public required string Example { set; get; }

    [JsonProperty("example_author")] public required string Author { set; get; }

    [JsonProperty("score")] public required string Votes { set; get; }
}