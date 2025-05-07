using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json.Linq;
using Serilog;
using SkEditor.API;

namespace SkEditor.Utilities.Docs.SkUnity;

public class SkUnityProvider : IDocProvider
{
    private const string BaseUri = "https://api.skunity.com/v1/%s/docs/";
    private static readonly Dictionary<string, AddonData> CachedAddons = [];

    private readonly HttpClient _client = new HttpClient()
        .WithUserAgent("SkEditor App");

    public DocProvider Provider => DocProvider.skUnity;

    public List<string> CanSearch(SearchData searchData)
    {
        if (searchData.Query.Length < 3 && string.IsNullOrEmpty(searchData.FilteredAddon) &&
            searchData.FilteredType == IDocumentationEntry.Type.All)
        {
            return [Translation.Get("DocumentationWindowInvalidDataQuery")];
        }

        return [];
    }

    public Task<IDocumentationEntry?> FetchElement(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<List<IDocumentationEntry>> Search(SearchData searchData)
    {
        // First build the URI
        string uri = BaseUri.Replace("%s", SkEditorAPI.Core.GetAppConfig().SkUnityApiKey) + "search/";
        List<string> queryElements = [];

        if (!string.IsNullOrEmpty(searchData.Query))
        {
            queryElements.Add(searchData.Query);
        }

        if (searchData.FilteredType != IDocumentationEntry.Type.All)
        {
            queryElements.Add("type:" + searchData.FilteredType.ToString().ToLower() + "s");
        }

        if (!string.IsNullOrEmpty(searchData.FilteredAddon))
        {
            queryElements.Add("addon:" + searchData.FilteredAddon);
        }

        uri += string.Join("%20", queryElements);

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
            await SkEditorAPI.Windows.ShowError(
                Translation.Get("DocumentationWindowErrorGlobal", response.ReasonPhrase));
            return [];
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        JObject responseObject = JObject.Parse(content);
        if (responseObject["response"]?.ToString() != "success")
        {
            await SkEditorAPI.Windows.ShowError(Translation.Get("DocumentationWindowErrorGlobal",
                responseObject["response"]?.ToString()));
            return [];
        }

        List<SkUnityDocEntry>? entries = responseObject["result"]?.ToObject<List<SkUnityDocEntry>>();
        return [.. entries ?? []];
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrEmpty(SkEditorAPI.Core.GetAppConfig().SkUnityApiKey);
    }

    public bool NeedsToLoadExamples => true;

    public async Task<List<IDocumentationExample>> FetchExamples(IDocumentationEntry entry)
    {
        string elementId = entry.Id;
        string uri = BaseUri.Replace("%s", SkEditorAPI.Core.GetAppConfig().SkUnityApiKey) + "getExamplesByID/" +
                     elementId;

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
            await SkEditorAPI.Windows.ShowError(
                Translation.Get("DocumentationWindowErrorGlobal", response.ReasonPhrase));
            return [];
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        JObject responseObject = JObject.Parse(content);
        JToken? result = responseObject["result"];

        // if result is a JArray, it means there are no examples
        if (result is JArray)
        {
            return [];
        }

        JObject? resultObject = responseObject["result"]?.ToObject<JObject>();
        if (resultObject == null) return [];

        List<string> keys = [];
        keys.AddRange(from key in resultObject.Properties() where int.TryParse(key.Name, out _) select key.Name);

        return keys
            .Select(key => resultObject[key]?.ToObject<SkUnityDocExample>())
            .OfType<IDocumentationExample>()
            .ToList();
    }

    public bool HasAddons => true;

    public async Task<List<string>> GetAddons()
    {
        if (CachedAddons.Count > 0)
        {
            return [.. CachedAddons.Keys];
        }

        string uri = BaseUri.Replace("%s", SkEditorAPI.Core.GetAppConfig().SkUnityApiKey) + "getAllAddons/";

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
            await SkEditorAPI.Windows.ShowError(
                Translation.Get("DocumentationWindowErrorGlobal", response.ReasonPhrase));
            return [];
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        JObject responseObject = JObject.Parse(content);
        JObject? addonsObj = responseObject["result"]?.ToObject<JObject>();
        if (addonsObj == null) return [];
        
        List<string> addons = addonsObj.Properties().Select(prop => prop.Name).ToList();

        foreach (string key in addons)
        {
            JToken? obj = addonsObj[key];
            Color color = Color.Parse("#" + obj?["colour"]?.ToObject<string>());
            string? forumResourceId = obj?["forums_resource_id"]?.ToObject<string>();
            if (forumResourceId == null)
            {
                Log.Warning("Addon {Addon} does not have a forum resource ID", key);
                continue;
            }
            CachedAddons[key] = new AddonData(color, forumResourceId);
        }

        return addons;
    }

    public IconSource Icon => new ImageIconSource
    {
        Source = new SvgImage
        {
            Source = SvgSource.LoadFromStream(AssetLoader.Open(new Uri("avares://SkEditor/Assets/Brands/skUnity.svg")))
        }
    };

    public async Task<Color?> GetAddonColor(string addonName)
    {
        AddonData? addon;
        if (CachedAddons.Count != 0)
        {
            return CachedAddons.TryGetValue(addonName, out addon) ? addon.Color : null;
        }

        try
        {
            await GetAddons();
        }
        catch (Exception e)
        {
            await SkEditorAPI.Windows.ShowError(e is TaskCanceledException
                ? Translation.Get("DocumentationWindowErrorOffline")
                : Translation.Get("DocumentationWindowErrorGlobal", e.Message));
            Log.Error(e, "Failed to fetch addons");
            return null;
        }

        return CachedAddons.TryGetValue(addonName, out addon) ? addon.Color : null;
    }

    public string GetLink(IDocumentationEntry entry)
    {
        return "https://docs.skunity.com/syntax/search/id:" + entry.Id;
    }

    public static IDocProvider Get()
    {
        return (SkUnityProvider)IDocProvider.Providers[DocProvider.skUnity];
    }

    public static string GetAddonLink(string addonName)
    {
        return "https://forums.skunity.com/resources/" + CachedAddons[addonName].ForumResourceId + "/";
    }

    private record AddonData(Color Color, string ForumResourceId);
}