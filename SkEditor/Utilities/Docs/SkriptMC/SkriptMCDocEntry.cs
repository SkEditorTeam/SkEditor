using Newtonsoft.Json;
using System;

namespace SkEditor.Utilities.Docs.SkriptMC;

/**
 * {
        "id": 1,
        "addon": "Skript",
        "name": "All Scripts (Tous les scripts)",
        "content": "Retourne tous les scripts, ou seulement ceux qui sont activ&eacute;s ou d&eacute;sactiv&eacute;s.",
        "version": "2.5",
        "example": "on script load:<br />\r\n&nbsp; &nbsp; send &quot;Voici la liste des addons actuellement charg&eacute;s : %enabled scripts%.&quot;",
        "pattern": "[all [of the]] scripts [(without ([subdirectory] paths|parents))]\r\n[all [of the]] (enabled|loaded) scripts [(without ([subdirectory] paths|parents))]\r\n[all [of the]] (disabled|unloaded) scripts [(without ([subdirectory] paths|parents))]",
        "category": "expressions",
        "documentationUrl": "https://skript-mc.fr/documentation/skript/expressions#all_scripts",
        "deprecation": null,
        "deprecationLink": null
    }
 */
public class SkriptMCDocEntry : IDocumentationEntry
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("content")]
    public string Description { get; set; }
    [JsonProperty("pattern")]
    public string Patterns { get; set; }
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("addon")]
    public string Addon { get; set; }
    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("example")] public string RawExample;

    public IDocumentationExample Example => new SkriptMCDocExample { Example = RawExample };

    public IDocumentationEntry.Type DocType
    {
        get => Enum.Parse<IDocumentationEntry.Type>(RawType[..^1], true);
        set => RawType = value.ToString().ToLower() + "s";
    }

    [JsonProperty("category")]
    public string RawType { get; set; }

    public DocProvider Provider => DocProvider.SkriptMC;

    public string? ReturnType { get; set; }
    public string? Changers { get; set; }
    public string? EventValues { get; set; }
}