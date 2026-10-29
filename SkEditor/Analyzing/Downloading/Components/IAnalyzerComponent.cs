using System.IO;
using System.Threading.Tasks;
using SkEditor.Core.Localization;

namespace SkEditor.Analyzing.Downloading.Components;

public interface IAnalyzerComponent
{
    TranslatableString Name { get; }
    string FileName { get; }
    ComponentFolder Folder { get; }

    Task<DownloadInfo?> GetDownloadInfoAsync();

    Task PrepareAsync(string filePath, string destinationFolder)
    {
        File.Move(filePath, Path.Combine(destinationFolder, FileName));
        return Task.CompletedTask;
    }
}

public enum ComponentFolder
{
    Binaries,
    Plugins
}