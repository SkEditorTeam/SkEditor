using System;
using System.Text.Json.Serialization;

namespace SkEditor.Utilities.Docs.SkUnity;

[Serializable]
public class SkUnityDocEntry : IDocumentationEntry
{
    [JsonPropertyName("doc")] public required string RawDoc { set; get; }

    [JsonPropertyName("name")] public required string Name { set; get; }

    [JsonPropertyName("desc")] public required string Description { set; get; }

    [JsonPropertyName("pattern")] public required string Patterns { set; get; }

    [JsonPropertyName("id")] public required string Id { set; get; }

    [JsonPropertyName("addon")] public required string Addon { set; get; }

    [JsonPropertyName("version")] public required string Version { set; get; }

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

    [JsonPropertyName("returntype")] public string? ReturnType { set; get; }

    [JsonPropertyName("changers")] public string? Changers { set; get; }

    [JsonPropertyName("eventvalues")] public string? EventValues { set; get; }

    public DocProvider Provider => DocProvider.skUnity;
}