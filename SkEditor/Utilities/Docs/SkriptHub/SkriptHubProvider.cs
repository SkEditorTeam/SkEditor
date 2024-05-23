using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using FluentAvalonia.UI.Controls;

namespace SkEditor.Utilities.Docs.SkriptHub;

public class SkriptHubProvider : IDocProvider
{
    public static IDocProvider Get() => IDocProvider.Providers[DocProvider.SkriptHub];

    private const string BaseUri = "https://skripthub.net/api/v1/";

    private readonly HttpClient _client = new();

    public DocProvider Provider => DocProvider.SkriptHub;
    public Task<IDocumentationEntry> FetchElement(string id)
    {
        throw new System.NotImplementedException();
    }

    public List<string> CanSearch(SearchData searchData)
    {
        if (searchData.Query.Length < 3 && string.IsNullOrEmpty(searchData.FilteredAddon) &&
            searchData.FilteredType == IDocumentationEntry.Type.All)
            return [Translation.Get("DocumentationWindowInvalidDataQuery")];

        return [];
    }

    public async Task<List<IDocumentationEntry>> Search(SearchData searchData)
    {
        var uri = BaseUri + "syntax/";
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
                ? Translation.Get("DocumentationWindowErrorOffline")
                : Translation.Get("DocumentationWindowErrorGlobal", e.Message));
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
            ApiVault.Get().ShowError(Translation.Get("DocumentationWindowErrorGlobal", response.ReasonPhrase));
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

    public bool NeedsToLoadExamples => true;

    public async Task<List<IDocumentationExample>> FetchExamples(IDocumentationEntry entry)
    {
        var elementId = entry.Id;
        var uri = BaseUri + "syntaxexample/" + "?syntax=" + elementId;

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
                ? Translation.Get("DocumentationWindowErrorOffline")
                : Translation.Get("DocumentationWindowErrorGlobal", e.Message));
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                ApiVault.Get().Log(await response.Content.ReadAsStringAsync(cancellationToken.Token));
            }
            catch (Exception e)
            {
                ApiVault.Get().Log(e.Message);
            }
            ApiVault.Get().ShowError(Translation.Get("DocumentationWindowErrorGlobal", response.ReasonPhrase));
            return [];
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        var elements = JsonConvert.DeserializeObject<List<SkriptHubDocExample>>(content);
        return elements.Cast<IDocumentationExample>().ToList();
    }

    public bool HasAddons => true;
    public async Task<List<string>> GetAddons()
    {
        var uri = BaseUri + "addon/";

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
                ? Translation.Get("DocumentationWindowErrorOffline")
                : Translation.Get("DocumentationWindowErrorGlobal", e.Message));
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                ApiVault.Get().Log(await response.Content.ReadAsStringAsync(cancellationToken.Token));
            }
            catch (Exception e)
            {
                ApiVault.Get().Log(e.Message);
            }
            ApiVault.Get().ShowError(Translation.Get("DocumentationWindowErrorGlobal", response.ReasonPhrase));
            return [];
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        var elements = JsonConvert.DeserializeObject<List<JObject>>(content);
        return elements.Select(e => e["name"].ToString()).ToList();
    }

    public IconSource Icon => new ImageIconSource()
    {
        Source = new SvgImage
        {
            Source = SvgSource.LoadFromStream(AssetLoader.Open(new Uri("avares://SkEditor/Assets/Brands/SkriptHub.svg")))
        }
    };
}