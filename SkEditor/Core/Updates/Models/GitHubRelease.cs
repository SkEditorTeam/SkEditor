using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SkEditor.Core.Updates.Models;

public sealed record GitHubRelease(
    [property: JsonPropertyName("tag_name")] string TagName,
    [property: JsonPropertyName("html_url")] string HtmlUrl,
    [property: JsonPropertyName("prerelease")] bool IsPrerelease,
    [property: JsonPropertyName("assets")] List<GitHubAsset> Assets
);

public sealed record GitHubAsset(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("digest")] string Digest,
    [property: JsonPropertyName("browser_download_url")] string DownloadUrl
);

[JsonSerializable(typeof(List<GitHubRelease>))]
public partial class GitHubReleaseJsonContext : JsonSerializerContext;