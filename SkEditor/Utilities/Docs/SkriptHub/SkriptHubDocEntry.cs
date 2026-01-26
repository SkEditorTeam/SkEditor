using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkEditor.Utilities.Docs.SkriptHub;

[Serializable]
public class SkriptHubDocEntry : IDocumentationEntry
{
    private string _addon = string.Empty;
    [JsonPropertyName("addon")] public required SkriptHubAddon? AddonObj { get; set; }

    [JsonPropertyName("syntax_type")] public required string RawDocType { get; set; }

    [JsonPropertyName("examples")] public required List<SkriptHubDocExample>? Examples { get; set; }

    [JsonPropertyName("title")] public required string Name { get; set; }

    [JsonPropertyName("description")] public required string Description { get; set; }

    [JsonPropertyName("syntax_pattern")] public required string Patterns { get; set; }

    [JsonPropertyName("id")] public required string Id { get; set; }

    [JsonPropertyName("addon_name")]
    public string Addon
    {
        get => AddonObj?.Name ?? _addon;
        set => _addon = value;
    }

    [JsonPropertyName("compatible_addon_version")]
    public required string Version { get; set; }

    [JsonIgnore]
    public IDocumentationEntry.Type DocType
    {
        get => Enum.Parse<IDocumentationEntry.Type>(RawDocType, true);
        set => RawDocType = value.ToString().ToLower();
    }

    [JsonPropertyName("return_type")] public string? ReturnType { get; set; }

    [JsonPropertyName("type_usage")] public string? Changers { get; set; }

    [JsonPropertyName("event_values")] public string? EventValues { get; set; }

    public DocProvider Provider => DocProvider.SkriptHub;

    public bool DoMatch(SearchData searchData)
    {
        if (!string.IsNullOrEmpty(searchData.FilteredAddon) && AddonObj?.Name != searchData.FilteredAddon)
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
    [JsonPropertyName("name")] public required string Name { get; set; }

    [JsonPropertyName("link_to_addon")] public required string Link { get; set; }
}

[JsonSerializable(typeof(SkriptHubDocEntry))]
[JsonSerializable(typeof(List<SkriptHubDocEntry>))]
[JsonSerializable(typeof(SkriptHubDocExample))]
[JsonSerializable(typeof(List<SkriptHubDocExample>))]
public partial class SkriptHubDocJsonContext : JsonSerializerContext;