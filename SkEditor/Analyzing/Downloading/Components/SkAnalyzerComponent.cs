using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Serilog;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;

namespace SkEditor.Analyzing.Downloading.Components;

public class SkAnalyzerComponent(HttpClient httpClient) : IAnalyzerComponent
{
    private const string SkAnalyzerTag = "v2.0.0-beta.7";
    
    public TranslatableString Name => "SkAnalyzer".ToTranslatable();
    
    public async Task<DownloadInfo?> GetDownloadInfoAsync()
    {
        const string apiUrl = $"https://api.github.com/repos/SkEditorTeam/SkAnalyzer/releases/tags/{SkAnalyzerTag}";

        await using Stream stream = await httpClient.GetStreamAsync(apiUrl);
        using JsonDocument document = await JsonDocument.ParseAsync(stream);

        JsonElement root = document.RootElement;
        JsonElement assets = root.GetProperty("assets");

        JsonElement? targetAsset = null;
        foreach (JsonElement asset in assets.EnumerateArray())
        {
            string? assetName = asset.GetProperty("name").GetString();
            if (assetName?.EndsWith(".jar") != true)
            {
                continue;
            }

            targetAsset = asset;
            break;
        }

        if (targetAsset is null)
        {
            Log.Error("Failed to find SkAnalyzer asset in GitHub release.");
            return null;
        }

        string downloadUrl = targetAsset.Value.GetProperty("browser_download_url").GetString()!;
        string name = targetAsset.Value.GetProperty("name").GetString()!;
        string checksum = targetAsset.Value.GetProperty("digest").GetString()!;
        long size = targetAsset.Value.GetProperty("size").GetInt64();
        
        return new DownloadInfo(name, downloadUrl, checksum, size);
    }
}