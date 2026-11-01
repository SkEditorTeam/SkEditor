using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SkEditor.Core.Common;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;

namespace SkEditor.Analyzing.Downloading.Components;

public class SkratchesComponent(IHttpClientFactory factory, ILogService logs) : IAnalyzerComponent
{
    private const string SkratchesTag = "build-16";

    public TranslatableString Name => "Skratches".ToTranslatable();
    public string FileName => "Skript.jar";
    public ComponentFolder Folder => ComponentFolder.Binaries;

    public async Task<DownloadInfo?> GetDownloadInfoAsync()
    {
        const string apiUrl = $"https://api.github.com/repos/SkEditorTeam/Skratches/releases/tags/{SkratchesTag}";

        using HttpClient client = factory.CreateClient(HttpClients.SkEditor);
        await using Stream stream = await client.GetStreamAsync(apiUrl);
        using JsonDocument document = await JsonDocument.ParseAsync(stream);

        JsonElement root = document.RootElement;
        JsonElement assets = root.GetProperty("assets");

        JsonElement? targetAsset = null;
        Version highestVersion = new(0, 0);

        foreach (JsonElement asset in assets.EnumerateArray())
        {
            string? assetName = asset.GetProperty("name").GetString();
            if (assetName is null || !assetName.EndsWith(".jar", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!TryExtractVersion(assetName, out Version? version) || version <= highestVersion)
            {
                continue;
            }

            highestVersion = version!;
            targetAsset = asset;
        }

        if (targetAsset is null)
        {
            logs.Error("Failed to find any valid Skratches asset in GitHub release.");
            return null;
        }

        string downloadUrl = targetAsset.Value.GetProperty("browser_download_url").GetString()!;
        string name = targetAsset.Value.GetProperty("name").GetString()!;
        string checksum = targetAsset.Value.GetProperty("digest").GetString()!;
        long size = targetAsset.Value.GetProperty("size").GetInt64();

        return new DownloadInfo(name, downloadUrl, checksum, size);
    }

    private static bool TryExtractVersion(string assetName, out Version? version)
    {
        version = null;

        int vIndex = assetName.IndexOf("-v", StringComparison.OrdinalIgnoreCase);
        int dashIndex = assetName.IndexOf("-skratched", StringComparison.OrdinalIgnoreCase);

        if (vIndex == -1 || dashIndex == -1 || dashIndex <= vIndex + 2)
        {
            return false;
        }

        string versionStr = assetName.Substring(vIndex + 2, dashIndex - (vIndex + 2));
        return Version.TryParse(versionStr, out version);
    }
}