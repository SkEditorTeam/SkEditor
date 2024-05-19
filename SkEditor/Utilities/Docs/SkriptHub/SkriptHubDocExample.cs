using System;
using Newtonsoft.Json;

namespace SkEditor.Utilities.Docs.SkriptHub;

[Serializable]
public class SkriptHubDocExample : IDocumentationExample
{
    [JsonProperty("example_code")]
    public string Example { set; get; }
    [JsonProperty("example_author")]
    public string Author { set; get; }
    [JsonProperty("score")]
    public string Votes { set; get; }
}