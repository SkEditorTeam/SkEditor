using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using SkEditor.API;
using SkEditor.Utilities.Localization;
using SkEditor.Views.Pages;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Utilities.Files;

internal static class FileCloser
{
    public static async Task CloseFile(TabViewTabCloseRequestedEventArgs e)
    {
        if (e.Tab.Content is WelcomePage && SkEditorAPI.Files.GetOpenedTabs().Count == 1)
            return;

        await SkEditorAPI.Files.Close(e.Tab);
    }

    public static async Task CloseCurrentFile()
    {
        OpenedFile? currentOpenedFile = SkEditorAPI.Files.GetCurrentOpenedFile();
        if (currentOpenedFile == null)
        {
            return;
        }

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
        return await SkEditorAPI.Windows.ShowDialog(Translation.Get(TranslationKey.Attention),
            Translation.Get(TranslationKey.ClosingFiles),
            new SymbolIconSource { Symbol = Symbol.Important, IconVariant = IconVariant.Filled });
    }
}