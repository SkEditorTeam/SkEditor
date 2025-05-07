using System;
using Newtonsoft.Json;

namespace SkEditor.Utilities.Docs.SkUnity;

[Serializable]
public class SkUnityDocExample : IDocumentationExample
{
    [JsonProperty("example")] public required string Example { get; set; }

    [JsonProperty("username")] public required string Author { get; set; }

    [JsonProperty("votes")] public required string Votes { get; set; }
}