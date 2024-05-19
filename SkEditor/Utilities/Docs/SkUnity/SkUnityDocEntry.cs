using System;
using Newtonsoft.Json;

namespace SkEditor.Utilities.Docs.SkUnity;

[Serializable]
public class SkUnityDocEntry : IDocumentationEntry
{
    
    [JsonProperty("name")]
    public string Name { set; get; }
    [JsonProperty("desc")]
    public string Description { set; get; }
    [JsonProperty("pattern")]
    public string Patterns { set; get; }
    [JsonProperty("id")]
    public string Id { set; get; }
    [JsonProperty("addon")]
    public string Addon { set; get; }
    [JsonProperty("version")]
    public string Version { set; get; }
    
    public IDocumentationEntry.Type DocType
    {
        get
        {
            try
            {
                return Enum.Parse<IDocumentationEntry.Type>(RawDoc, true);
            }
            catch (Exception e)
            {
                return Enum.Parse<IDocumentationEntry.Type>(RawDoc[..^1], true);
            }
        }
        set => RawDoc = value.ToString();
    }

    [JsonProperty("doc")]
    public string RawDoc { set; get; }
    
    [JsonProperty("returntype")]
    public string? ReturnType { set; get; }
    [JsonProperty("changers")]
    public string? Changers { set; get; }
    [JsonProperty("eventvalues")]
    public string? EventValues { set; get; }
    
    public DocProvider Provider => DocProvider.SkUnity;
}