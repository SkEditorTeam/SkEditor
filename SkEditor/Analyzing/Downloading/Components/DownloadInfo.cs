using System.IO;

namespace SkEditor.Analyzing.Downloading.Components;

public record DownloadInfo(string Name, string Url, string Checksum, long Size);