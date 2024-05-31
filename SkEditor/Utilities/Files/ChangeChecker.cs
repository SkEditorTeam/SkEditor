using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Files;
public class ChangeChecker
{
    private static Dictionary<TextEditor, string> lastKnownContentDictionary = [];
    private static string GetLastKnownContent(TextEditor textEditor) =>
        lastKnownContentDictionary.TryGetValue(textEditor, out string lastKnownContent) ? lastKnownContent : "";
    private static void SetLastKnownContent(TextEditor textEditor, string content) => lastKnownContentDictionary[textEditor] = content;

    private static bool isMessageShown = false;
    public static Dictionary<string, bool> HasChangedDictionary { get; } = [];

    public static async void Check()
    {
        if (!SkEditorAPI.Core.GetAppConfig().CheckForChanges) return;

        try
        {
            if (!ApiVault.Get().IsFileOpen()) return;

            TabViewItem item = ApiVault.Get().GetTabView().SelectedItem as TabViewItem;
            if (item.Tag == null) return;
            if (string.IsNullOrWhiteSpace(item.Tag.ToString())) return;
            string path = Uri.UnescapeDataString(item.Tag.ToString());
            if (!File.Exists(path)) return;

            if (!HasChangedDictionary.TryGetValue(path, out bool hasChanged))
            {
                HasChangedDictionary[path] = false;
                hasChanged = false;
            }
            if (hasChanged) return;

            string textToWrite = ApiVault.Get().GetTextEditor().Document.Text;
            using StreamReader reader = new(path);
            string textToRead = reader.ReadToEnd();

            if (textToWrite.Equals(textToRead) ||
                textToRead.Equals(GetLastKnownContent(ApiVault.Get().GetTextEditor()))) return;

            if (isMessageShown) return;
            isMessageShown = true;
            await ShowMessage(item, textToRead);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while checking for changes");
        }

        isMessageShown = false;
    }

    private static async Task ShowMessage(TabViewItem item, string textToRead)
    {
        var openedFile = SkEditorAPI.Files.GetOpenedFiles().Find(tab => tab.TabViewItem == item);
        var result = await SkEditorAPI.Windows.ShowDialog(
            Translation.Get("Attention"),
            Translation.Get("ChangesDetected"),
            new SymbolIconSource { Symbol = Symbol.ImportantFilled },
            primaryButtonText: "Yes",
            cancelButtonText: "No");


        if (result == ContentDialogResult.Primary)
        {
            openedFile.Editor.Document.Text = textToRead;
            SetLastKnownContent(ApiVault.Get().GetTextEditor(), textToRead);
            openedFile.IsSaved = true;
        }
        else
        {
            SetLastKnownContent(openedFile.Editor, textToRead);
            openedFile.IsSaved = false;
        }
    }
}
