using ExCSS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SkEditor.Views.Marketplace.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using SkEditor.API;

namespace SkEditor.Views.Marketplace;
public class MarketplaceLoader
{
    private static readonly string[] supportedTypes = ["FileSyntax", "Theme", "Addon", "ThemeWithSyntax", "ZipAddon"];
    private static readonly string[] hiddenItems = ["Shadow", "Analyzer"];

    public static async IAsyncEnumerable<MarketplaceItem> GetItems()
    {
        string url = MarketplaceWindow.MarketplaceUrl + "/items.json";
        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();
            string[] itemNames = JsonConvert.DeserializeObject<string[]>(json);
            foreach (string itemName in itemNames)
            {
                if (hiddenItems.Contains(itemName)) continue;

                MarketplaceItem item = GetItem(itemName);
                if (item.ItemName == null) continue;
                if (!supportedTypes.Contains(item.ItemType)) continue;
                yield return item;
            }
        }
        else
        {
            Log.Error("Failed to get items.json");
        }

        yield break;
    }

    private static MarketplaceItem GetItem(string name)
    {
        string url = MarketplaceWindow.MarketplaceUrl + "/items/" + name;
        string manifestUrl = url + "/manifest.json";
        using HttpClient client = new();
        HttpResponseMessage response = client.GetAsync(manifestUrl).Result;
        if (response.IsSuccessStatusCode)
        {
            string json = response.Content.ReadAsStringAsync().Result;
            MarketplaceItem item = JsonConvert.DeserializeObject<MarketplaceItem>(json, new MarketplaceItemConverter());
            item = FormatUrls(url, item);
            return item;
        }
        else
        {
            Log.Error("Failed to get manifest.json for item " + name);
            return new MarketplaceItem();
        }
    }

    private static MarketplaceItem FormatUrls(string url, MarketplaceItem item)
    {
        static string CombineUrls(string baseUrl, string relativeUrl) => baseUrl + "/" + relativeUrl;

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
            themeWithSyntaxItem.SyntaxFolders = themeWithSyntaxItem.SyntaxFolders.Select(x => CombineUrls(url, x)).ToArray();
        }

        return item;
    }
}

public class MarketplaceItem
{
    [JsonProperty("name")]
    public string ItemName { get; set; }
    [JsonProperty("type")]
    public string ItemType { get; set; }

    [JsonProperty("shortDescription")]
    public string ItemShortDescription { get; set; }

    [JsonProperty("icon")]
    public string ItemImageUrl { get; set; }

    [JsonProperty("longDescription")]
    public string ItemLongDescription { get; set; }

    [JsonProperty("version")]
    public string ItemVersion { get; set; }

    [JsonProperty("author")]
    public string ItemAuthor { get; set; }

    public virtual void Install() { }
    public virtual void Uninstall() { }

    public virtual bool IsInstalled()
    {
        return false;
    }
}

public class MarketplaceItemConverter : JsonConverter<MarketplaceItem>
{
    public override MarketplaceItem ReadJson(JsonReader reader, Type objectType, MarketplaceItem existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);
        string itemType = jsonObject["type"]?.Value<string>();

        var defaultSerializer = new JsonSerializer();

        return itemType switch
        {
            "FileSyntax" => jsonObject.ToObject<SyntaxItem>(defaultSerializer),
            "Theme" => jsonObject.ToObject<ThemeItem>(defaultSerializer),
            "Addon" => jsonObject.ToObject<AddonItem>(defaultSerializer),
            "ThemeWithSyntax" => jsonObject.ToObject<ThemeWithSyntaxItem>(defaultSerializer),
            "ZipAddon" => jsonObject.ToObject<ZipAddonItem>(defaultSerializer),
            _ => new MarketplaceItem(),
        };
    }

    public override void WriteJson(JsonWriter writer, MarketplaceItem value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}