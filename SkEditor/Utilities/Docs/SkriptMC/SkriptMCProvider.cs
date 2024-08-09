using Avalonia.Media;
using FluentAvalonia.UI.Controls;
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
        var uri = BaseUri.Replace("%s", SkEditorAPI.Core.GetAppConfig().SkriptMCAPIKey) + "&articleName=" + searchData.Query;

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
            await SkEditorAPI.Windows.ShowError(e is TaskCanceledException
                ? Translation.Get("DocumentationWindowErrorOffline")
                : Translation.Get("DocumentationWindowErrorGlobal", e.Message));
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                await SkEditorAPI.Windows.ShowError(Translation.Get("DocumentationWindowSkriptMCBad2"));
                return [];
            }

            //SkEditorAPI.Windows.ShowError($"An error occurred while fetching the documentation.\n\n{response.ReasonPhrase}");
            await SkEditorAPI.Windows.ShowError(Translation.Get("DocumentationWindowErrorGlobal", response.ReasonPhrase));
            return [];
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        var entries = JsonConvert.DeserializeObject<List<SkriptMCDocEntry>>(content);
        return entries.Cast<IDocumentationEntry>().ToList();
    }

    public List<string> CanSearch(SearchData searchData)
    {
        if (searchData.Query.Length < 3 && string.IsNullOrEmpty(searchData.FilteredAddon) && searchData.FilteredType == IDocumentationEntry.Type.All)
            return [Translation.Get("DocumentationWindowInvalidDataQuery")];

        if (searchData.FilteredType == IDocumentationEntry.Type.All)
            return [Translation.Get("DocumentationWindowSkriptMCBad")];

        return [];
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrEmpty(SkEditorAPI.Core.GetAppConfig().SkriptMCAPIKey);
    }

    public bool NeedsToLoadExamples => false;
    public Task<List<IDocumentationExample>> FetchExamples(IDocumentationEntry entry)
    {
        var example = (entry as SkriptMCDocEntry)?.Example;
        return Task.FromResult<List<IDocumentationExample>>(example == null ? [] : [example]);
    }

    public bool HasAddons => false;
    public Task<List<string>> GetAddons() => Task.FromResult(new List<string>());

    public Task<Color?> GetAddonColor(string addonName) => Task.FromResult<Color?>(null);

    public IconSource Icon => new BitmapIconSource() { UriSource = new("avares://SkEditor/Assets/Brands/SkriptMC.png") };

    public string? GetLink(IDocumentationEntry entry)
    {
        return null;
    }
}