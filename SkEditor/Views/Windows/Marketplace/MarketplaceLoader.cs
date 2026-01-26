using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
        string json = await client.GetStringAsync(url);

        string[] itemNames = JsonSerializer.Deserialize(json, MarketplaceContext.Default.StringArray) ?? [];

        foreach (string itemName in itemNames)
        {
            if (HiddenItems.Contains(itemName))
            {
                continue;
            }

            MarketplaceItem? item = await GetItem(itemName);
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

    private static async Task<MarketplaceItem?> GetItem(string name)
    {
        string url = $"{MarketplaceWindow.MarketplaceUrl}/items/{name}/manifest.json";
        using HttpClient client = new();
        try
        {
            string json = await client.GetStringAsync(url);
            MarketplaceItem? item = JsonSerializer.Deserialize(json, MarketplaceContext.Default.MarketplaceItem);

            return item == null ? null : FormatUrls($"{MarketplaceWindow.MarketplaceUrl}/items/{name}", item);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get manifest.json for item {Name}", name);
            return null;
        }
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

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(SyntaxItem), "NewSyntax")]
[JsonDerivedType(typeof(ThemeItem), "Theme")]
[JsonDerivedType(typeof(AddonItem), "Addon")]
[JsonDerivedType(typeof(ThemeWithSyntaxItem), "NewThemeWithSyntax")]
[JsonDerivedType(typeof(ZipAddonItem), "ZipAddon")]
public class MarketplaceItem
{
    [JsonPropertyName("name")] public required string ItemName { get; set; }

    [JsonIgnore] public virtual string ItemType => "Unknown";

    [JsonPropertyName("shortDescription")] public required string ItemShortDescription { get; set; }

    [JsonPropertyName("icon")] public required string ItemImageUrl { get; set; }

    [JsonPropertyName("longDescription")] public required string ItemLongDescription { get; set; }

    [JsonPropertyName("version")] public required string ItemVersion { get; set; }

    [JsonPropertyName("author")] public required string ItemAuthor { get; set; }

    [JsonIgnore] public MarketplaceWindow? Marketplace { get; set; }

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

[JsonSourceGenerationOptions(
    WriteIndented = true,
    AllowOutOfOrderMetadataProperties = true
)]
[JsonSerializable(typeof(string[]))]
[JsonSerializable(typeof(MarketplaceItem))]
[JsonSerializable(typeof(SyntaxItem))]
[JsonSerializable(typeof(ThemeItem))]
[JsonSerializable(typeof(AddonItem))]
[JsonSerializable(typeof(ThemeWithSyntaxItem))]
[JsonSerializable(typeof(ZipAddonItem))]
internal partial class MarketplaceContext : JsonSerializerContext;