using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using SkEditor.API;

namespace SkEditor.Utilities.Docs.SkriptMC;

public class SkriptMcProvider : IDocProvider
{
    private const string BaseUri = "https://skript-mc.fr/api/documentation/search?api_key=%s";

    private readonly HttpClient _client = new();

    public DocProvider Provider => DocProvider.SkriptMC;

    public Task<IDocumentationEntry?> FetchElement(string id)
    {
        return Task.FromResult<IDocumentationEntry?>(null);
    }

    public async Task<List<IDocumentationEntry>> Search(SearchData searchData)
    {
        string uri = BaseUri.Replace("%s", SkEditorAPI.Core.GetAppConfig().SkriptMcApiKey) + "&articleName=" +
                     searchData.Query;

        uri += "&categorySlug=" + searchData.FilteredType.ToString().ToLower() + "s";
        uri += "&addonSlug=" + (string.IsNullOrEmpty(searchData.FilteredAddon) ? "Skript" : searchData.FilteredAddon);

        CancellationTokenSource cancellationToken = new(new TimeSpan(0, 0, 5));
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

            await SkEditorAPI.Windows.ShowError(
                Translation.Get("DocumentationWindowErrorGlobal", response.ReasonPhrase));
            return [];
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        List<SkriptMcDocEntry>? entries = JsonConvert.DeserializeObject<List<SkriptMcDocEntry>>(content);
        return entries?.Cast<IDocumentationEntry>()?.ToList()
               ?? [];
    }

    public List<string> CanSearch(SearchData searchData)
    {
        if (searchData.Query.Length < 3 && string.IsNullOrEmpty(searchData.FilteredAddon) &&
            searchData.FilteredType == IDocumentationEntry.Type.All)
        {
            return [Translation.Get("DocumentationWindowInvalidDataQuery")];
        }

        if (searchData.FilteredType == IDocumentationEntry.Type.All)
        {
            return [Translation.Get("DocumentationWindowSkriptMCBad")];
        }

        return [];
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrEmpty(SkEditorAPI.Core.GetAppConfig().SkriptMcApiKey);
    }

    public bool NeedsToLoadExamples => false;

    public Task<List<IDocumentationExample>> FetchExamples(IDocumentationEntry entry)
    {
        IDocumentationExample? example = (entry as SkriptMcDocEntry)?.Example;
        return Task.FromResult<List<IDocumentationExample>>(example == null ? [] : [example]);
    }

    public bool HasAddons => false;

    public Task<List<string>> GetAddons()
    {
        return Task.FromResult(new List<string>());
    }

    public Task<Color?> GetAddonColor(string addonName)
    {
        return Task.FromResult<Color?>(null);
    }

    public IconSource Icon => new BitmapIconSource
        { UriSource = new Uri("avares://SkEditor/Assets/Brands/SkriptMC.png") };

    public string? GetLink(IDocumentationEntry entry)
    {
        return null;
    }
}