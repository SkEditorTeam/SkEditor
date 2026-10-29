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

public class AnalyzerDownloadHelper : IAnalyzerDownloadHelper
{
    public readonly IAnalyzerComponent[] Components;

    private readonly string _downloadPath;
    
    private readonly IHttpClientFactory _factory;
    private readonly IFileSystem _fileSystem;
    private readonly IAnalyzer _analyzer;

    public AnalyzerDownloadHelper(IHttpClientFactory factory,
        IFileSystem fileSystem,
        IAnalyzer analyzer)
    {
        _factory = factory;
        _fileSystem = fileSystem;
        _analyzer = analyzer;
        _downloadPath = Path.Combine(analyzer.BinDir);
        
        Components = [
            new JavaRuntimeComponent(factory),
            new SkAnalyzerComponent(factory),
            new SkratchesComponent(factory)
        ];
        
        IsReady.Value = Components.All(CheckComponentExists);
    }

    public BindableReactiveProperty<bool> IsReady { get; } = new();
    
    public BindableReactiveProperty<bool> IsDownloading { get; } = new();
    public BindableReactiveProperty<float> Progress { get; } = new();
    public BindableReactiveProperty<TranslatableString> ProgressDescription { get; } = new();

    public bool CheckComponents()
    {
        bool ready = Components.All(CheckComponentExists);
        IsReady.Value = ready;
        return ready;
    }
    
    private bool CheckComponentExists(IAnalyzerComponent component)
    {
        string dest = GetDestinationFolder(component.Folder);
        string file = Path.Combine(dest, component.FileName);

        return _fileSystem.File.Exists(file) || _fileSystem.Directory.Exists(file);
    }

    public async Task Download()
    {
        if (IsDownloading.Value) return;
        
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

            CheckComponents();
        }
    }

    private string GetDestinationFolder(ComponentFolder folderType)
    {
        return folderType switch
        {
            ComponentFolder.Binaries => _analyzer.BinDir,
            ComponentFolder.Plugins => Path.Combine(_analyzer.WorkingDir, "plugins"),
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
            using HttpClient client = _factory.CreateClient("AnalyzerDownload");
            await using (Stream downloadStream = await client.GetStreamAsync(info.Url, ct))
            await using (ProgressStream progressStream = new(downloadStream, progress))
            await using (FileSystemStream fileStream = _fileSystem.File.Create(tempPath))
            {
                await progressStream.CopyToAsync(fileStream, ct);
            }

            _fileSystem.File.Move(tempPath, filePath, true);
        }
        catch
        {
            if (_fileSystem.File.Exists(tempPath))
            {
                try
                {
                    _fileSystem.File.Delete(tempPath);
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