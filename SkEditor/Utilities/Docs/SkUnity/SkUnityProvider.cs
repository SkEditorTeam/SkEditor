using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Docs.SkUnity;

public class SkUnityProvider : IDocProvider
{
    private const string BaseUri = "https://api.skunity.com/v1/%s/docs/";
    private static readonly Dictionary<string, AddonData> CachedAddons = [];

    private readonly HttpClient _client = new HttpClient()
        .WithUserAgent("SkEditor App");

    public DocProvider Provider => DocProvider.SkUnity;

    public List<string> CanSearch(SearchData searchData)
    {
        if (searchData.Query.Length < 3 && string.IsNullOrEmpty(searchData.FilteredAddon) &&
            searchData.FilteredType == IDocumentationEntry.Type.All)
        {
            return [Translation.Get(TranslationKey.DocumentationWindowInvalidDataQuery)];
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
        string uri = BaseUri.Replace("%s", SkEditorAPI.Configs.GetSecretsConfig().SkUnityApiKey) + "search/";
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
                ? Translation.Get(TranslationKey.DocumentationWindowErrorOffline)
                : Translation.Get(TranslationKey.DocumentationWindowErrorGlobal, e.Message));
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            await SkEditorAPI.Windows.ShowError(
                Translation.Get(TranslationKey.DocumentationWindowErrorGlobal, response.ReasonPhrase));
            return [];
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        JsonNode? responseObject = JsonNode.Parse(content);
        if (responseObject?["response"]?.GetValue<string>() != "success")
        {
            await SkEditorAPI.Windows.ShowError(Translation.Get(TranslationKey.DocumentationWindowErrorGlobal,
                responseObject?["response"]?.GetValue<string>()));
            return [];
        }

        List<SkUnityDocEntry>? entries =
            responseObject["result"]?.Deserialize(SkUnityContext.Default.ListSkUnityDocEntry);
        return [.. entries ?? []];
    }

    public bool IsAvailable()
    {
        return !string.IsNullOrEmpty(SkEditorAPI.Configs.GetSecretsConfig().SkUnityApiKey);
    }

    public bool NeedsToLoadExamples => true;

    public async Task<List<IDocumentationExample>> FetchExamples(IDocumentationEntry entry)
    {
        string elementId = entry.Id;
        string uri = BaseUri.Replace("%s", SkEditorAPI.Configs.GetSecretsConfig().SkUnityApiKey) + "getExamplesByID/" +
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
                ? Translation.Get(TranslationKey.DocumentationWindowErrorOffline)
                : Translation.Get(TranslationKey.DocumentationWindowErrorGlobal, e.Message));
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            await SkEditorAPI.Windows.ShowError(
                Translation.Get(TranslationKey.DocumentationWindowErrorGlobal, response.ReasonPhrase));
            return [];
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        JsonNode? responseObject = JsonNode.Parse(content);
        JsonNode? resultNode = responseObject?["result"];

        if (resultNode is JsonArray or null)
        {
            return [];
        }

        JsonObject resultObject = resultNode.AsObject();
        List<IDocumentationExample> examples = [];

        foreach (KeyValuePair<string, JsonNode?> property in resultObject)
        {
            if (!int.TryParse(property.Key, out _))
            {
                continue;
            }

            SkUnityDocExample? example = property.Value.Deserialize(SkUnityContext.Default.SkUnityDocExample);
            if (example != null)
            {
                examples.Add(example);
            }
        }

        return examples;
    }

    public bool HasAddons => true;

    public async Task<List<string>> GetAddons()
    {
        if (CachedAddons.Count > 0)
        {
            return [.. CachedAddons.Keys];
        }

        string uri = BaseUri.Replace("%s", SkEditorAPI.Configs.GetSecretsConfig().SkUnityApiKey) + "getAllAddons/";

        CancellationTokenSource cancellationToken = new(new TimeSpan(0, 0, 5));
        HttpResponseMessage response;
        try
        {
            response = await _client.GetAsync(uri, cancellationToken.Token);
        }
        catch (Exception e)
        {
            await SkEditorAPI.Windows.ShowError(e is TaskCanceledException
                ? Translation.Get(TranslationKey.DocumentationWindowErrorOffline)
                : Translation.Get(TranslationKey.DocumentationWindowErrorGlobal, e.Message));
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            await SkEditorAPI.Windows.ShowError(
                Translation.Get(TranslationKey.DocumentationWindowErrorGlobal, response.ReasonPhrase));
            return [];
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken.Token);
        JsonNode? responseObject = JsonNode.Parse(content);
        JsonObject? addonsObj = responseObject?["result"]?.AsObject();

        if (addonsObj == null)
        {
            return [];
        }

        List<string> addons = [];
        foreach ((string key, JsonNode? obj) in addonsObj)
        {
            addons.Add(key);

            string? hexColor = obj?["colour"]?.GetValue<string>();
            string? forumResourceId = obj?["forums_resource_id"]?.GetValue<string>();

            if (forumResourceId == null)
            {
                Log.Warning("Addon {Addon} does not have a forum resource ID", key);
                continue;
            }

            Color color = Color.Parse("#" + hexColor);
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
                ? Translation.Get(TranslationKey.DocumentationWindowErrorOffline)
                : Translation.Get(TranslationKey.DocumentationWindowErrorGlobal, e.Message));
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
        return (SkUnityProvider)IDocProvider.Providers[DocProvider.SkUnity];
    }

    public static string GetAddonLink(string addonName)
    {
        return "https://forums.skunity.com/resources/" + CachedAddons[addonName].ForumResourceId + "/";
    }

    private record AddonData(Color Color, string ForumResourceId);
}

[JsonSerializable(typeof(List<SkUnityDocEntry>))]
[JsonSerializable(typeof(SkUnityDocEntry))]
[JsonSerializable(typeof(SkUnityDocExample))]
internal partial class SkUnityContext : JsonSerializerContext;