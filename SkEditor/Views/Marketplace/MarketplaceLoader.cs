using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace SkEditor.Views.Marketplace;
public class MarketplaceLoader
{
	private static readonly string[] supportedTypes = [ "Syntax highlighting", "Theme", "Addon" ];

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
			MarketplaceItem item = JsonConvert.DeserializeObject<MarketplaceItem>(json);
			item.ItemImageUrl = url + "/" + item.ItemImageUrl;
			item.ItemFileUrl = url + "/" + item.ItemFileUrl;
			return item;
		}
		else
		{
			Log.Error("Failed to get manifest.json for item " + name);
			return new MarketplaceItem();
		}
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

	[JsonProperty("file")]
	public string ItemFileUrl { get; set; }

	[JsonProperty("longDescription")]
	public string ItemLongDescription { get; set; }

	[JsonProperty("version")]
	public string ItemVersion { get; set; }

	[JsonProperty("author")]
	public string ItemAuthor { get; set; }

	[JsonProperty("requiresRestart")]
	public bool ItemRequiresRestart { get; set; }
}