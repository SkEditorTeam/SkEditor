using System.Threading.Tasks;
using SkEditor.Core.Localization;

namespace SkEditor.Analyzing.Downloading.Components;

public interface IAnalyzerComponent
{
    TranslatableString Name { get; }
    ComponentFolder Folder { get; }

    Task<DownloadInfo?> GetDownloadInfoAsync();

    Task PrepareAsync(string filePath, string destinationFolder);
}

public enum ComponentFolder
{
    Binaries,
    Plugins
}