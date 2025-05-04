using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SkEditor.Utilities.Docs.SkriptHub;

[Serializable]
public class SkriptHubDocEntry : IDocumentationEntry
{
    [JsonIgnore] private string _addon;

    [JsonProperty("addon")] public SkriptHubAddon? AddonObj { get; set; }

    [JsonProperty("syntax_type")] public string RawDocType { get; set; }

    [JsonProperty("examples")] public List<SkriptHubDocExample>? Examples { get; set; }

    [JsonProperty("title")] public string Name { get; set; }

    [JsonProperty("description")] public string Description { get; set; }

    [JsonProperty("syntax_pattern")] public string Patterns { get; set; }

    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("addon_name")]
    public string Addon
    {
        get => AddonObj?.Name ?? _addon;
        set => _addon = value;
    }


    [JsonProperty("compatible_addon_version")]
    public string Version { get; set; }

    [JsonIgnore]
    public IDocumentationEntry.Type DocType
    {
        get => Enum.Parse<IDocumentationEntry.Type>(RawDocType, true);
        set => RawDocType = value.ToString().ToLower();
    }

    [JsonProperty("return_type")] public string? ReturnType { get; set; }

    [JsonProperty("type_usage")] public string? Changers { get; set; }

    [JsonProperty("event_values")] public string? EventValues { get; set; }

    public DocProvider Provider => DocProvider.SkriptHub;

    public bool DoMatch(SearchData searchData)
    {
        if (!string.IsNullOrEmpty(searchData.FilteredAddon) && AddonObj.Name != searchData.FilteredAddon)
        {
            return false;
        }

        if (searchData.FilteredType != IDocumentationEntry.Type.All && DocType != searchData.FilteredType)
        {
            return false;
        }

        return Name.Contains(searchData.Query, StringComparison.CurrentCultureIgnoreCase);
    }
}

[Serializable]
public class SkriptHubAddon
{
    [JsonProperty("name")] public string Name { get; set; }

    [JsonProperty("link_to_addon")] public string Link { get; set; }
}