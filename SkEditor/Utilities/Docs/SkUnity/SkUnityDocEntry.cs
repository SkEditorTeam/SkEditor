using System;
using Newtonsoft.Json;

namespace SkEditor.Utilities.Docs.SkUnity;

[Serializable]
public class SkUnityDocEntry : IDocumentationEntry
{
    [JsonProperty("doc")] public required string RawDoc { set; get; }

    [JsonProperty("name")] public required string Name { set; get; }

    [JsonProperty("desc")] public required string Description { set; get; }

    [JsonProperty("pattern")] public required string Patterns { set; get; }

    [JsonProperty("id")] public required string Id { set; get; }

    [JsonProperty("addon")] public required string Addon { set; get; }

    [JsonProperty("version")] public required string Version { set; get; }

    public IDocumentationEntry.Type DocType
    {
        get
        {
            if (RawDoc.Equals("classes"))
            {
                return IDocumentationEntry.Type.Type; // This is a type now
            }

            try
            {
                return Enum.Parse<IDocumentationEntry.Type>(RawDoc, true);
            }
            catch
            {
                return Enum.Parse<IDocumentationEntry.Type>(RawDoc[..^1], true);
            }
        }
        set => RawDoc = value.ToString();
    }

    [JsonProperty("returntype")] public string? ReturnType { set; get; }

    [JsonProperty("changers")] public string? Changers { set; get; }

    [JsonProperty("eventvalues")] public string? EventValues { set; get; }

    public DocProvider Provider => DocProvider.skUnity;
}