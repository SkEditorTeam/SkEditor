using System;
using Newtonsoft.Json;

namespace SkEditor.Utilities.Docs.SkUnity;

[Serializable]
public class SkUnityDocExample : IDocumentationExample
{
    [JsonProperty("example")]
    public string Example { get; set; }
    [JsonProperty("username")]
    public string Author { get; set; }
    [JsonProperty("votes")]
    public string Votes { get; set; }
}