using System;
using System.Text.Json.Serialization;

namespace SkEditor.Utilities.Docs.SkUnity;

[Serializable]
public class SkUnityDocExample : IDocumentationExample
{
    [JsonPropertyName("example")]
    public required string Example { get; set; }

    [JsonPropertyName("username")]
    public required string Author { get; set; }

    [JsonPropertyName("votes")]
    public required string Votes { get; set; }
}