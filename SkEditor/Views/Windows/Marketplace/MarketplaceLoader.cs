using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SkEditor.Views.Windows.Marketplace.Types;

namespace SkEditor.Views.Windows.Marketplace;

public class MarketplaceLoader
{
    private static readonly string[] SupportedTypes = ["NewSyntax", "Theme", "Addon", "NewThemeWithSyntax", "ZipAddon"];
    private static readonly string[] HiddenItems = [];

    public static async IAsyncEnumerable<MarketplaceItem> GetItems()
    {
        const string url = MarketplaceWindow.MarketplaceUrl + "/items.json";
        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            string[] itemNames = JsonConvert.DeserializeObject<string[]>(json) ?? [];
            foreach (string itemName in itemNames)
            {
                if (HiddenItems.Contains(itemName))
                {
                    continue;
                }

                MarketplaceItem? item = GetItem(itemName);
                if (item?.ItemName == null)
                {
                    continue;
                }

                if (!SupportedTypes.Contains(item.ItemType))
                {
                    continue;
                }

                yield return item;
            }
        }
        else
        {
            Log.Error("Failed to get items.json");
        }
    }

    private static MarketplaceItem? GetItem(string name)
    {
        string url = MarketplaceWindow.MarketplaceUrl + "/items/" + name;
        string manifestUrl = url + "/manifest.json";
        using HttpClient client = new();
        HttpResponseMessage response = client.GetAsync(manifestUrl).Result;
        if (response.IsSuccessStatusCode)
        {
            string json = response.Content.ReadAsStringAsync().Result;
            MarketplaceItem? item = JsonConvert.DeserializeObject<MarketplaceItem>(json, new MarketplaceItemConverter());
            if (item == null)
            {
                Log.Error("Failed to deserialize manifest.json for item {Name}", name);
                return null;
            }
            item = FormatUrls(url, item);
            return item;
        }

        Log.Error("Failed to get manifest.json for item {Name}", name);
        return null;
    }

    private static MarketplaceItem FormatUrls(string url, MarketplaceItem item)
    {
        static string CombineUrls(string baseUrl, string relativeUrl)
        {
            return baseUrl + "/" + relativeUrl;
        }

        item.ItemImageUrl = CombineUrls(url, item.ItemImageUrl);
        if (item is AddonItem addonItem)
        {
            addonItem.ItemFileUrl = CombineUrls(url, addonItem.ItemFileUrl);
        }
        else if (item is ThemeItem themeItem)
        {
            themeItem.ItemFileUrl = CombineUrls(url, themeItem.ItemFileUrl);
        }
        else if (item is SyntaxItem syntaxItem)
        {
            syntaxItem.ItemSyntaxFolders = syntaxItem.ItemSyntaxFolders.Select(x => CombineUrls(url, x)).ToArray();
        }
        else if (item is ThemeWithSyntaxItem themeWithSyntaxItem)
        {
            themeWithSyntaxItem.ThemeFileUrl = CombineUrls(url, themeWithSyntaxItem.ThemeFileUrl);
            themeWithSyntaxItem.SyntaxFolders =
                themeWithSyntaxItem.SyntaxFolders.Select(x => CombineUrls(url, x)).ToArray();
        }

        return item;
    }
}

public class MarketplaceItem
{
    [JsonProperty("name")] public required string ItemName { get; set; }

    [JsonProperty("type")] public required string ItemType { get; set; }

    [JsonProperty("shortDescription")] public required string ItemShortDescription { get; set; }

    [JsonProperty("icon")] public required string ItemImageUrl { get; set; }

    [JsonProperty("longDescription")] public required string ItemLongDescription { get; set; }

    [JsonProperty("version")] public required string ItemVersion { get; set; }

    [JsonProperty("author")] public required string ItemAuthor { get; set; }
    
    [JsonProperty("links")] public MarketplaceLinksItem? ItemLinks { get; set; }

    [JsonIgnore] public required MarketplaceWindow Marketplace { get; set; }

    public virtual Task Install()
    {
        return Task.CompletedTask;
    }

    public virtual Task Uninstall()
    {
        return Task.CompletedTask;
    }

    public virtual bool IsInstalled()
    {
        return false;
    }
}

public class MarketplaceLinksItem
{
    [JsonProperty("issuesUrl")] public string? ItemIssuesUrl { get; set; }
    
    [JsonProperty("repositoryUrl")] public string? ItemRepositoryUrl { get; set; }
    
    [JsonProperty("changelogUrl")] public string? ItemChangelogUrl { get; set; }
    
    [JsonProperty("documentationUrl")] public string? ItemDocumentationUrl { get; set; }
}

public class MarketplaceItemConverter : JsonConverter<MarketplaceItem>
{
    public override MarketplaceItem? ReadJson(JsonReader reader, Type objectType, MarketplaceItem? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);
        string? itemType = jsonObject["type"]?.Value<string>();

        JsonSerializer defaultSerializer = new();

        return itemType switch
        {
            "NewSyntax" => jsonObject.ToObject<SyntaxItem>(defaultSerializer),
            "Theme" => jsonObject.ToObject<ThemeItem>(defaultSerializer),
            "Addon" => jsonObject.ToObject<AddonItem>(defaultSerializer),
            "NewThemeWithSyntax" => jsonObject.ToObject<ThemeWithSyntaxItem>(defaultSerializer),
            "ZipAddon" => jsonObject.ToObject<ZipAddonItem>(defaultSerializer),
            _ => null
        };
    }

    public override void WriteJson(JsonWriter writer, MarketplaceItem? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}