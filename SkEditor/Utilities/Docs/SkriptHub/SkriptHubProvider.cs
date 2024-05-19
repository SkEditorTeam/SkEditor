using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkEditor.API;

namespace SkEditor.Utilities.Docs.SkriptHub;

public class SkriptHubProvider : IDocProvider
{
    public static IDocProvider Get() => new SkriptHubProvider();
    
    private const string BaseUri
        = "https://skripthub.net/api/v1/syntax/";// ?search=member&addons=Skript

    private readonly HttpClient _client = new();
    
    public DocProvider Provider => DocProvider.SkriptHub;
    public Task<IDocumentationEntry> FetchElement(string id)
    {
        throw new System.NotImplementedException();
    }

    public List<string> CanSearch(SearchData searchData)
    {
        if (searchData.Query.Length < 3 && string.IsNullOrEmpty(searchData.FilteredAddon) && searchData.FilteredType == IDocumentationEntry.Type.All)
            return ["Query must be at least 3 characters long"];
        
        return [];
    }

    public async Task<List<IDocumentationEntry>> Search(SearchData searchData)
    {
        var uri = BaseUri;
        var parameters = new List<string>();
        
        if (!string.IsNullOrEmpty(searchData.Query))
            parameters.Add("search=" + searchData.Query);
        if (searchData.FilteredType != IDocumentationEntry.Type.All)
            parameters.Add("type=" + searchData.FilteredType.ToString().ToLower());
        if (!string.IsNullOrEmpty(searchData.FilteredAddon))
            parameters.Add("addon=" + searchData.FilteredAddon);
        
        uri += "?" + string.Join("&", parameters);
        
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", "Token " + ApiVault.Get().GetAppConfig().SkriptHubAPIKey);
            
        var cancellationToken = new CancellationTokenSource(new TimeSpan(0, 0, 5));
        HttpResponseMessage response;
        try
        {
            response = await _client.GetAsync(uri, cancellationToken.Token);
        }
        catch (Exception e)
        {
            ApiVault.Get().ShowError(e is TaskCanceledException
                ? "The request to the documentation server timed out. Are the docs down?"
                : $"An error occurred while fetching the documentation.\n\n{e.Message}");
            return [];
        }
        
        if (!response.IsSuccessStatusCode)
        {
            try
            {
                ApiVault.Get().Log(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                ApiVault.Get().Log(e.Message);
            }
            ApiVault.Get().ShowError($"The documentation server returned an error: {response.StatusCode}");
            return [];
        }
        
        var content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        var elements = JsonConvert.DeserializeObject<List<SkriptHubDocEntry>>(content);
        return elements.Cast<IDocumentationEntry>().ToList();
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrEmpty(ApiVault.Get().GetAppConfig().SkriptHubAPIKey);
    }
}