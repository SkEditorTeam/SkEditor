using System;
using System.Text.Json.Serialization;

namespace SkEditor.Utilities.Docs.SkriptHub;

[Serializable]
public class SkriptHubDocExample : IDocumentationExample
{
    [JsonPropertyName("example_code")]
    public required string Example { set; get; }

    [JsonPropertyName("example_author")]
    public required string Author { set; get; }

    [JsonPropertyName("score")]
    public required string Votes { set; get; }
}