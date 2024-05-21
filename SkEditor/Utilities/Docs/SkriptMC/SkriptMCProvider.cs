using Newtonsoft.Json;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Docs.SkriptMC;

public class SkriptMCProvider : IDocProvider
{
    private const string BaseUri = "https://skript-mc.fr/api/documentation/search?api_key=%s";

    private readonly HttpClient _client = new();

    public DocProvider Provider => DocProvider.SkriptMC;

    public Task<IDocumentationEntry> FetchElement(string id)
    {
        return null;
    }

    public async Task<List<IDocumentationEntry>> Search(SearchData searchData)
    {
        var uri = BaseUri.Replace("%s", ApiVault.Get().GetAppConfig().SkriptMCAPIKey) + "&articleName=" + searchData.Query;

        uri += "&categorySlug=" + searchData.FilteredType.ToString().ToLower() + "s";
        uri += "&addonSlug=" + (string.IsNullOrEmpty(searchData.FilteredAddon) ? "Skript" : searchData.FilteredAddon);

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
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                ApiVault.Get().ShowError("SkriptMC is currently not available for documentation. Please try again later.");
                return [];
            }

            ApiVault.Get().ShowError($"An error occurred while fetching the documentation.\n\n{response.ReasonPhrase}");
            return [];
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        var entries = JsonConvert.DeserializeObject<List<SkriptMCDocEntry>>(content);
        return entries.Cast<IDocumentationEntry>().ToList();
    }

    public List<string> CanSearch(SearchData searchData)
    {
        if (searchData.Query.Length < 3 && string.IsNullOrEmpty(searchData.FilteredAddon) && searchData.FilteredType == IDocumentationEntry.Type.All)
            return ["Query must be at least 3 characters long"];

        if (searchData.FilteredType == IDocumentationEntry.Type.All)
            return ["SkriptMC does not support searching for all types"];

        return [];
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrEmpty(ApiVault.Get().GetAppConfig().SkriptMCAPIKey);
    }

    public bool NeedsToLoadExamples => false;
    public Task<List<IDocumentationExample>> FetchExamples(IDocumentationEntry entry)
    {
        var example = (entry as SkriptMCDocEntry)?.Example;
        return Task.FromResult<List<IDocumentationExample>>(example == null ? [] : [example]);
    }

    public bool HasAddons => false;
    public async Task<List<string>> GetAddons()
    {
        return [];
    }
}