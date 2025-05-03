using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Extensions;
using SkEditor.Views;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkEditor.Utilities;
public static class FileDownloader
{
    private const string ZipUrl = MarketplaceWindow.MarketplaceUrl + "SkEditorFiles/Items_and_json.zip";
    private static readonly string AppDataFolderPath = AppConfig.AppDataFolderPath;
    private static readonly string ZipFilePath = Path.Combine(AppDataFolderPath, "items.zip");

    public static async Task CheckForMissingItemFiles(Window visual)
    {
        string itemsFile = Path.Combine(AppConfig.AppDataFolderPath, "items.json");
        if (File.Exists(itemsFile)) return;

        TaskDialog td = CreateTaskDialog(visual);
        var result = await td.ShowAsync();

        TaskDialogStandardResult standardResult = (TaskDialogStandardResult)result;
        if (standardResult == TaskDialogStandardResult.Cancel)
        {
            visual.Close();
            await SkEditorAPI.Windows.ShowError(Translation.Get("DownloadMissingFilesFailed"));
        }
    }

    private static TaskDialog CreateTaskDialog(Visual visual)
    {
        var td = new TaskDialog
        {
            Title = Translation.Get("DownloadingMissingFilesTitle"),
            ShowProgressBar = true,
            IconSource = new SymbolIconSource { Symbol = Symbol.Download },
            SubHeader = Translation.Get("Downloading"),
            Content = Translation.Get("DownloadingMissingFilesDescriptionItems"),
        };

        td.Opened += async (_, _) => await DownloadMissingFiles(td);

        td.XamlRoot = visual;
        return td;
    }

    private static async Task DownloadMissingFiles(TaskDialog td)
    {
        const TaskDialogProgressState state = TaskDialogProgressState.Normal;
        td.SetProgressBarState(0, state);

        try
        {
            using (HttpClient client = new())
            {
                var progress = new Progress<float>();
                progress.ProgressChanged += (_, sender) => td.SetProgressBarState(sender, state);

                await using var file = new FileStream(ZipFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await client.DownloadDataAsync(ZipUrl, file, progress);
            }

            ZipFile.ExtractToDirectory(ZipFilePath, AppDataFolderPath);
            File.Delete(ZipFilePath);

            Dispatcher.UIThread.Post(() => { td.Hide(TaskDialogStandardResult.OK); });
        }
        catch
        {
            Dispatcher.UIThread.Post(() => { td.Hide(TaskDialogStandardResult.Cancel); });
        }
    }
}