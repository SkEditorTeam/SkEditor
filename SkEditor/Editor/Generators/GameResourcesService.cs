using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MemoryPack;
using SkEditor.Core.Common;
using SkEditor.Core.Logging;
using SkEditor.Editor.Generators.Models;

namespace SkEditor.Editor.Generators;

public class GameResourcesService(IHttpClientFactory clientFactory, ILogService logs)
    : IGameResourcesService
{
    private const string AssetUrl = "https://api.github.com/repos/SkEditorTeam/SkEditorAssetGenerator/releases/latest";

    private static readonly string ResourcesFolder = Path.Combine(AppPaths.LocalData, "Resources");
    private static readonly string ItemsBinPath = Path.Combine(ResourcesFolder, "items.bin");

    public async Task<bool> DownloadResources(CancellationToken ct)
    {
        if (File.Exists(ItemsBinPath))
        {
            logs.Info("Game resources already exist, skipping download.");
            return true;
        }
        
        using HttpClient client = clientFactory.CreateClient(HttpClients.SkEditor);
        await using Stream stream = await client.GetStreamAsync(AssetUrl, ct);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);

        JsonElement root = document.RootElement;
        JsonElement assets = root.GetProperty("assets");

        JsonElement? targetAsset = null;
        foreach (JsonElement asset in assets.EnumerateArray())
        {
            string? assetName = asset.GetProperty("name").GetString();
            if (assetName != "items.bin")
            {
                continue;
            }

            targetAsset = asset;
            break;
        }

        if (targetAsset is null)
        {
            logs.Error("Failed to find items.bin asset in GitHub release.");
            return false;
        }

        string downloadUrl = targetAsset.Value.GetProperty("browser_download_url").GetString()!;
        string checksum = targetAsset.Value.GetProperty("digest").GetString()!;

        using HttpResponseMessage response = await client.GetAsync(downloadUrl, ct);
        response.EnsureSuccessStatusCode();
        
        Directory.CreateDirectory(ResourcesFolder);
        await using FileStream fileStream = File.Create(ItemsBinPath);
        await response.Content.CopyToAsync(fileStream, ct);

        return true;
    }

    public async Task<Item[]?> GetItemsAsync(CancellationToken ct)
    {
        if (!File.Exists(ItemsBinPath))
        {
            return null;
        }
        
        try
        {
            await using FileStream binFileStream = File.OpenRead(ItemsBinPath);
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Item).TypeHandle);
            ItemCache? cache = await MemoryPackSerializer.DeserializeAsync<ItemCache>(binFileStream, cancellationToken: ct);

            return cache?.Items;
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to load game resources.");

            try
            {
                File.Delete(ItemsBinPath);
            }
            catch (Exception deleteEx)
            {
                logs.Error(deleteEx, "Failed to delete corrupted items.bin file.");
            }
            
            return null;
        }
    }
}