using System;
using Newtonsoft.Json;

namespace SkEditor.Utilities.Docs.SkriptHub;

/*
 * {
    "id": 919,
    "creator": "bensku",
    "title": "Region Members & Owners",
    "description": "A list of members or owners of a region.\nThis expression requires a supported regions plugin to be installed.",
    "syntax_pattern": "(all|the|) (members|owner[s]) of [[the] region[s]] %regions%\n[[the] region[s]] %regions%'[s] (members|owner[s])",
    "compatible_addon_version": "2.1",
    "compatible_minecraft_version": null,
    "syntax_type": "expression",
    "required_plugins": [],
    "addon": "Skript",
    "type_usage": null,
    "return_type": "Offline Player",
    "event_values": null,
    "event_cancellable": false,
    "link": "http://skripthub.net/docs/?id=919",
    "created_at": "2017-10-04T00:46:01.931364Z",
    "updated_at": "2019-09-30T12:31:32.754472Z",
    "entries": null
  },
 */
[Serializable]
public class SkriptHubDocEntry : IDocumentationEntry
{
    
    [JsonProperty("title")]
    public string Name { get; set; }
    [JsonProperty("description")]
    public string Description { get; set; }
    [JsonProperty("syntax_pattern")]
    public string Patterns { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("addon")]
    public string Addon { get; set; }
    [JsonProperty("compatible_addon_version")]
    public string Version { get; set; }
    
    [JsonIgnore]
    public IDocumentationEntry.Type DocType { 
        get => Enum.Parse<IDocumentationEntry.Type>(RawDocType, true);
        set => RawDocType = value.ToString().ToLower();
    }
    
    [JsonProperty("syntax_type")]
    public string RawDocType { get; set; }
    
    [JsonProperty("return_type")]
    public string? ReturnType { get; set; }
    [JsonProperty("type_usage")]
    public string? Changers { get; set; }
    [JsonProperty("event_values")]
    public string? EventValues { get; set; }
    
    public DocProvider Provider => DocProvider.SkriptHub;
    
}