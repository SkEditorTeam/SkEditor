using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;

namespace SkEditor.Analyzing.Downloading.Components;

public class JavaRuntimeComponent(HttpClient httpClient) : IAnalyzerComponent
{
    public TranslatableString Name => "JRE".ToTranslatable();
    
    public async Task<DownloadInfo?> GetDownloadInfoAsync()
    {
        string mapUrl = GetLatestDownloadMapUrl();

        await using var responseStream = await httpClient.GetStreamAsync(mapUrl);
        using JsonDocument document = await JsonDocument.ParseAsync(responseStream);

        JsonElement root = document.RootElement;
        if (root.GetArrayLength() == 0) return null;

        JsonElement package = root[0]
            .GetProperty("binary")
            .GetProperty("package");

        string link = package.GetProperty("link").GetString()!;
        string name = package.GetProperty("name").GetString()!;
        string checksum = package.GetProperty("checksum").GetString()!;
        long size = package.GetProperty("size").GetInt64();
        
        return new DownloadInfo(name, link, checksum, size);
    }
    
    private static string GetLatestDownloadMapUrl()
    {
        string os = OperatingSystem.IsWindows() ? "windows" :
            OperatingSystem.IsLinux() ? "linux" :
            OperatingSystem.IsMacOS() ? "mac" : 
            throw new PlatformNotSupportedException();

        string arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "aarch64",
            _ => throw new PlatformNotSupportedException()
        };

        return $"https://api.adoptium.net/v3/assets/latest/25/hotspot?os={os}&architecture={arch}&image_type=jre";
    }
}