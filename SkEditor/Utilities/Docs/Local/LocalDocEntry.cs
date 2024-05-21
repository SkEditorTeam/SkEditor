using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkEditor.Utilities.Docs.Local;

[Serializable]
public class LocalDocEntry : IDocumentationEntry
{
    public LocalDocEntry() { }

    public LocalDocEntry(IDocumentationEntry other, List<IDocumentationExample> examples)
    {
        Name = other.Name;
        Description = other.Description;
        Patterns = other.Patterns;
        Id = other.Id;
        Addon = other.Addon;
        Version = other.Version;
        DocType = other.DocType;
        ReturnType = other.ReturnType;
        Changers = other.Changers;
        EventValues = other.EventValues;

        Examples = examples.ConvertAll(x => new LocalDocExample(x));
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public string Patterns { get; set; }
    public string Id { get; set; }
    public string Addon { get; set; }
    public string Version { get; set; }
    public IDocumentationEntry.Type DocType { get; set; }

    [JsonIgnore]
    public DocProvider Provider => DocProvider.Local;
    public string? ReturnType { get; set; }
    public string? Changers { get; set; }
    public string? EventValues { get; set; }

    public List<LocalDocExample> Examples { get; set; } = new();

    public bool DoMatch(SearchData searchData)
    {
        if (searchData.FilteredType != IDocumentationEntry.Type.All && DocType != searchData.FilteredType)
            return false;

        if (!string.IsNullOrEmpty(searchData.FilteredAddon) && Addon != searchData.FilteredAddon)
            return false;

        if (!string.IsNullOrEmpty(searchData.Query) && !Name.Contains(searchData.Query, StringComparison.OrdinalIgnoreCase)
                                                    && !Description.Contains(searchData.Query, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }
}