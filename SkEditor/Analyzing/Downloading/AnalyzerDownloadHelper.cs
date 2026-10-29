using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using R3;
using SkEditor.Analyzing.Downloading.Components;
using SkEditor.Core.Common;
using SkEditor.Core.Localization;

namespace SkEditor.Analyzing.Downloading;

public class AnalyzerDownloadHelper(
    HttpClient httpClient,
    IFileSystem fileSystem,
    IAnalyzer analyzer)
    : IAnalyzerDownloadHelper
{
    public readonly IAnalyzerComponent[] Components =
    [
        new JavaRuntimeComponent(httpClient),
        new SkAnalyzerComponent(httpClient),
        new SkratchesComponent(httpClient)
    ];

    private readonly string _downloadPath = Path.Combine(analyzer.BinDir);
    
    public BindableReactiveProperty<bool> IsDownloading { get; } = new();
    public BindableReactiveProperty<float> Progress { get; } = new();
    public BindableReactiveProperty<TranslatableString> ProgressDescription { get; } = new();

    public bool CheckComponents()
    {
        return Components.All(CheckComponentExists);
    }
    
    private bool CheckComponentExists(IAnalyzerComponent component)
    {
        string dest = GetDestinationFolder(component.Folder);
        string file = Path.Combine(dest, component.FileName);

        return fileSystem.File.Exists(file) || fileSystem.Directory.Exists(file);
    }

    public async Task Download()
    {
        IsDownloading.Value = true;
        try
        {
            foreach (IAnalyzerComponent component in Components)
            {
                if (CheckComponentExists(component)) continue;
                
                Progress.Value = 0;
                ProgressDescription.Value = new TranslatableString(TranslationKey.SettingsAnalyzerDownloading, component.Name);
            
                DownloadInfo? info = await DownloadFileAsync(component, Progress, CancellationToken.None);
                if (info is null) continue;

                string filePath = Path.Combine(_downloadPath, info.Name);
                string dest = GetDestinationFolder(component.Folder);
                Directory.CreateDirectory(dest);
                await component.PrepareAsync(filePath, dest);
            }
        }
        finally
        {
            Progress.Value = 0;
            IsDownloading.Value = false;
        }
    }

    private string GetDestinationFolder(ComponentFolder folderType)
    {
        return folderType switch
        {
            ComponentFolder.Binaries => analyzer.BinDir,
            ComponentFolder.Plugins => Path.Combine(analyzer.WorkingDir, "plugins"),
            _ => throw new ArgumentOutOfRangeException(nameof(folderType), folderType, null)
        };
    }

    private async Task<DownloadInfo?> DownloadFileAsync(IAnalyzerComponent component, BindableReactiveProperty<float> progressProperty,
        CancellationToken ct)
    {
        DownloadInfo? info = await component.GetDownloadInfoAsync();
        if (info is null)
        {
            return null;
        }

        Progress<long> progress = new(bytes =>
        {
            float bytesPercent = (float)bytes / info.Size;
            float percent = bytesPercent * 100f;
            progressProperty.Value = percent;
        });

        Directory.CreateDirectory(_downloadPath);
        string filePath = Path.Combine(_downloadPath, info.Name);
        string tempPath = filePath + ".tmp";

        try
        {
            await using (Stream downloadStream = await httpClient.GetStreamAsync(info.Url, ct))
            await using (ProgressStream progressStream = new(downloadStream, progress))
            await using (FileSystemStream fileStream = fileSystem.File.Create(tempPath))
            {
                await progressStream.CopyToAsync(fileStream, ct);
            }

            fileSystem.File.Move(tempPath, filePath, true);
        }
        catch
        {
            if (fileSystem.File.Exists(tempPath))
            {
                try
                {
                    fileSystem.File.Delete(tempPath);
                }
                catch
                {
                    // ignore
                }
            }

            throw;
        }

        return info;
    }
}