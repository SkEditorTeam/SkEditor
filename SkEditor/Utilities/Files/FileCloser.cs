using FluentAvalonia.UI.Controls;
using SkEditor.API;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Files;
internal static class FileCloser
{
    public static async void CloseFile(TabViewTabCloseRequestedEventArgs e) =>
        await SkEditorAPI.Files.Close(e.Tab);
    public static async void CloseCurrentFile() =>
        await SkEditorAPI.Files.Close(SkEditorAPI.Files.GetCurrentOpenedFile());

    public static async void CloseAllFiles()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary)
            return;

        SkEditorAPI.Files.BatchClose(IFiles.FileCloseAction.All);
    }

    public static async void CloseAllExceptCurrent()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary)
            return;

        SkEditorAPI.Files.BatchClose(IFiles.FileCloseAction.AllExceptCurrent);
    }

    public static async void CloseUnsaved()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary)
            return;

        SkEditorAPI.Files.BatchClose(IFiles.FileCloseAction.Unsaved);
    }

    public static async void CloseAllToTheLeft()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary)
            return;

        SkEditorAPI.Files.BatchClose(IFiles.FileCloseAction.AllLeft);
    }

    public static async void CloseAllToTheRight()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary) return;

        SkEditorAPI.Files.BatchClose(IFiles.FileCloseAction.AllRight);
    }

    private static async Task<ContentDialogResult> ShowConfirmationDialog() =>
        await SkEditorAPI.Windows.ShowDialog(Translation.Get("Attention"),
            Translation.Get("ClosingFiles"), new SymbolIconSource { Symbol = Symbol.ImportantFilled });
}