using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkEditor.API;

namespace SkEditor.Utilities.Docs.SkUnity;

public class SkUnityProvider : IDocProvider
{
    private const string BaseUri 
        = "https://api.skunity.com/v1/90ca3ceffde58bc4258e2c3d011748d1/docs/search/";

    private readonly HttpClient _client = new ();
    
    public DocProvider Provider => DocProvider.SkUnity;
    public List<string> CanSearch(SearchData searchData)
    {
        if (searchData.Query.Length < 3 && searchData.FilteredAddons.Count == 0 && searchData.FilteredTypes.Count == 0)
            return ["Query must be at least 3 characters long"];
        
        return [];
    }

    public Task<IDocumentationEntry> FetchElement(string id)
    {
        throw new System.NotImplementedException();
    }

    public async Task<List<IDocumentationEntry>> Search(SearchData searchData)
    {
        // First build the URI
        var uri = BaseUri;
        var queryElements = new List<string>();
        
        if (!string.IsNullOrEmpty(searchData.Query))
            queryElements.Add(searchData.Query);
        if (searchData.FilteredAddons.Count > 0)
            searchData.FilteredAddons.ToList().ForEach(a => queryElements.Add("addon:" + a));
        if (searchData.FilteredTypes.Count > 0)
            searchData.FilteredTypes.Select(t => t.ToString().ToLower() + "s").ToList().ForEach(t => queryElements.Add("doc:" + t));
        
        uri += string.Join("+", queryElements);
        
        // Now fetch the data
        var response = await _client.GetAsync(uri);
        if (!response.IsSuccessStatusCode)
        {
            ApiVault.Get().ShowError($"An error occurred while fetching the documentation.\n\nReceived status code: {response.StatusCode}");
            return new List<IDocumentationEntry>();
        }
        
        var content = await response.Content.ReadAsStringAsync();
        var responseObject = JObject.Parse(content);
        if (responseObject["response"].ToString() != "success")
        {
            ApiVault.Get().ShowError($"An error occurred while fetching the documentation.\n\nReceived response: {responseObject["response"]}");
            return new List<IDocumentationEntry>();
        }
        var entries = responseObject["result"].ToObject<List<SkUnityDocEntry>>();
        return entries.ToList<IDocumentationEntry>();
    }
    
    public static IDocProvider Get() => (SkUnityProvider) IDocProvider.Providers[DocProvider.SkUnity];
}