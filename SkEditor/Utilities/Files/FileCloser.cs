using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using SkEditor.API;

namespace SkEditor.Utilities.Files;

internal static class FileCloser
{
    public static async Task CloseFile(TabViewTabCloseRequestedEventArgs e)
    {
        await SkEditorAPI.Files.Close(e.Tab);
    }

    public static async Task CloseCurrentFile()
    {
        OpenedFile? currentOpenedFile = SkEditorAPI.Files.GetCurrentOpenedFile();
        if (currentOpenedFile == null) return;
        await SkEditorAPI.Files.Close(currentOpenedFile);
    }

    public static async Task CloseAllFiles()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary)
        {
            return;
        }

        await SkEditorAPI.Files.BatchClose(IFiles.FileCloseAction.All);
    }

    public static async Task CloseAllExceptCurrent()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary)
        {
            return;
        }

        await SkEditorAPI.Files.BatchClose(IFiles.FileCloseAction.AllExceptCurrent);
    }

    public static async Task CloseUnsaved()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary)
        {
            return;
        }

        await SkEditorAPI.Files.BatchClose(IFiles.FileCloseAction.Unsaved);
    }

    public static async Task CloseAllToTheLeft()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary)
        {
            return;
        }

        await SkEditorAPI.Files.BatchClose(IFiles.FileCloseAction.AllLeft);
    }

    public static async Task CloseAllToTheRight()
    {
        if (await ShowConfirmationDialog() != ContentDialogResult.Primary)
        {
            return;
        }

        await SkEditorAPI.Files.BatchClose(IFiles.FileCloseAction.AllRight);
    }

    private static async Task<ContentDialogResult> ShowConfirmationDialog()
    {
        return await SkEditorAPI.Windows.ShowDialog(Translation.Get("Attention"),
            Translation.Get("ClosingFiles"), new SymbolIconSource { Symbol = Symbol.ImportantFilled });
    }
}