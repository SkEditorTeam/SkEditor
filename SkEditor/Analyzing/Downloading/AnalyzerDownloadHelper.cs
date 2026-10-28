using System;
using System.IO;
using System.IO.Abstractions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentIcons.Common;
using R3;
using SkEditor.Analyzing.Downloading.Components;
using SkEditor.Core.Common;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Dialogs.Models.Elements;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;

namespace SkEditor.Analyzing.Downloading;

public class AnalyzerDownloadHelper(
    HttpClient httpClient,
    IFileSystem fileSystem,
    IAnalyzer analyzer,
    IDialogService dialogs)
    : IAnalyzerDownloadHelper
{
    public readonly IAnalyzerComponent[] Components =
    [
        new JavaRuntimeComponent(httpClient),
        new SkAnalyzerComponent(httpClient),
        new SkratchesComponent(httpClient)
    ];

    public async Task Download()
    {
        BindableReactiveProperty<float> progressProperty = new();
        BindableReactiveProperty<TranslatableString> descProperty = new();

        ProgressDialogElement progressElement = new()
        {
            Value = progressProperty
        };

        TextDialogElement descriptionElement = new()
        {
            Text = descProperty
        };

        Dialog dialog = Dialog
            .Create("Downloading...".ToTranslatable())
            .WithIcon(Symbol.ArrowDownload)
            .WithElements(progressElement, descriptionElement)
            .WithButtons(DialogButton.Cancel);

        await dialogs.RunDialogAsync(dialog, async ct =>
        {
            foreach (IAnalyzerComponent component in Components)
            {
                descProperty.Value = $"Downloading {component.Name}...".ToTranslatable();
                progressProperty.Value = 0;
                await DownloadFileAsync(component, progressProperty, ct);
            }
        });
    }

    private async Task DownloadFileAsync(IAnalyzerComponent component, BindableReactiveProperty<float> progressProperty,
        CancellationToken ct)
    {
        DownloadInfo? info = await component.GetDownloadInfoAsync();
        if (info is null)
        {
            return;
        }

        Progress<long> progress = new(bytes =>
        {
            float bytesPercent = (float)bytes / info.Size;
            float percent = bytesPercent * 100f;
            progressProperty.Value = percent;
        });

        string filePath = Path.Combine(analyzer.BinDir, info.Name);
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
    }
}