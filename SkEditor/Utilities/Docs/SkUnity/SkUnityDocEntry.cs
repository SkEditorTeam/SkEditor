using Newtonsoft.Json;

namespace SkEditor.Utilities.Docs.SkUnity;

/*
 * {
            "id": "4842",
            "name": "On Join",
            "doc": "events",
            "desc": "Called when the player joins the server. The player is already in a world when this event is called, so if you want to prevent players from joining you should prefer on connect over this event.",
            "addon": "Skript",
            "version": "1.0",
            "pattern": "[on] [player] (login|logging in|join[ing])",
            "plugin": "",
            "eventvalues": "event-player\nevent-world",
            "changers": "",
            "returntype": "",
            "is_array": "0",
            "tags": "",
            "reviewed": "true",
            "versions": "",
            "checkout_json_id": "",
            "docs_score": "7.000",
            "has_snippet": "0",
            "keywords": null
        },
 */
public class SkUnityDocEntry : IDocumentationEntry
{
    
    [JsonProperty("name")]
    public string Name { get; }
    [JsonProperty("desc")]
    public string Description { get; }
    [JsonProperty("pattern")]
    public string Patterns { get; }
    [JsonProperty("id")]
    public string Id { get; }
    [JsonProperty("addon")]
    public string Addon { get; }
    [JsonProperty("version")]
    public string Version { get; }
    [JsonProperty("doc")]
    public IDocumentationEntry.Type DocType { get; }
    
    [JsonProperty("returntype")]
    public string? ReturnType { get; }
    [JsonProperty("changers")]
    public string? Changers { get; }
    [JsonProperty("eventvalues")]
    public string? EventValues { get; }
    
    public DocProvider Provider => DocProvider.SkUnity;
}