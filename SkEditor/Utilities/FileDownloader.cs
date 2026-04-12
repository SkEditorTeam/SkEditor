using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using SkEditor.Services.Localization;
using SkEditor.Utilities.Localization;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Utilities;

public static class FileDownloader
{
    // private const string ZipUrl = MarketplaceWindow.MarketplaceUrl + "SkEditorFiles/Items_and_json.zip";
    // private static readonly string AppDataFolderPath = SkEditorAPI.Configs.GetAppDataPath();
    // private static readonly string ZipFilePath = Path.Combine(AppDataFolderPath, "items.zip");

    public static async Task CheckForMissingItemFiles(Window visual)
    {
        // string itemsFile = Path.Combine(SkEditorAPI.Configs.GetAppDataPath(), "items.json");
        // if (File.Exists(itemsFile))
        // {
        //     return;
        // }
        //
        // TaskDialog td = CreateTaskDialog(visual);
        // object? result = await td.ShowAsync();
        //
        // TaskDialogStandardResult standardResult = (TaskDialogStandardResult)result;
        // if (standardResult == TaskDialogStandardResult.Cancel)
        // {
        //     visual.Close();
        //     await SkEditorAPI.Windows.ShowError(Translation.Get(TranslationKey.DownloadMissingFilesFailed));
        // }
    }

    private static FATaskDialog CreateTaskDialog(Visual visual)
    {
        FATaskDialog td = new()
        {
            Title = TranslationService.Current.Get(TranslationKey.DownloadingMissingFilesTitle),
            ShowProgressBar = true,
            IconSource = new SymbolIconSource { Symbol = Symbol.ArrowDownload },
            SubHeader = TranslationService.Current.Get(TranslationKey.Downloading),
            Content = TranslationService.Current.Get(TranslationKey.DownloadingMissingFilesDescriptionItems)
        };

        td.Opened += async (_, _) => await DownloadMissingFiles(td);

        td.XamlRoot = visual;
        return td;
    }

    private static async Task DownloadMissingFiles(FATaskDialog td)
    {
        const FATaskDialogProgressState state = FATaskDialogProgressState.Normal;
        td.SetProgressBarState(0, state);

        try
        {
            // using (HttpClient client = new())
            // {
            //     Progress<float> progress = new();
            //     progress.ProgressChanged += (_, sender) => td.SetProgressBarState(sender, state);
            //
            //     await using FileStream file = new(ZipFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            //     // await client.DownloadDataAsync(ZipUrl, file, progress);
            // }
            //
            // await ZipFile.ExtractToDirectoryAsync(ZipFilePath, AppDataFolderPath);
            // File.Delete(ZipFilePath);

            Dispatcher.UIThread.Post(() => { td.Hide(FATaskDialogStandardResult.OK); });
        }
        catch
        {
            Dispatcher.UIThread.Post(() => { td.Hide(FATaskDialogStandardResult.Cancel); });
        }
    }
}