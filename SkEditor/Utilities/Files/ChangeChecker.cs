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
    private static Dictionary<OpenedFile, string> lastKnownContentDictionary = [];
    private static string GetLastKnownContent(OpenedFile openedFile) =>
        lastKnownContentDictionary.TryGetValue(openedFile, out string lastKnownContent) ? lastKnownContent : "";
    private static void SetLastKnownContent(OpenedFile openedFile, string content) => lastKnownContentDictionary[openedFile] = content;

    private static bool isMessageShown = false;
    public static Dictionary<string, bool> HasChangedDictionary { get; } = [];

    public static async void Check()
    {
        if (!SkEditorAPI.Core.GetAppConfig().CheckForChanges) return;

        try
        {
            OpenedFile file = SkEditorAPI.Files.GetCurrentOpenedFile();
            if (!file.IsEditor || string.IsNullOrWhiteSpace(file.Path)) return;

            string path = Uri.UnescapeDataString(file.Path);
            if (!File.Exists(path)) return;

            if (!HasChangedDictionary.TryGetValue(path, out bool hasChanged))
            {
                HasChangedDictionary[path] = false;
                hasChanged = false;
            }
            if (hasChanged) return;

            string textToWrite = file.Editor.Document.Text;
            using StreamReader reader = new(path);
            string textToRead = reader.ReadToEnd();

            if (textToWrite.Equals(textToRead) ||
                textToRead.Equals(GetLastKnownContent(file))) return;

            if (isMessageShown) return;
            isMessageShown = true;
            await ShowMessage(file, textToRead);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while checking for changes");
        }

        isMessageShown = false;
    }

    private static async Task ShowMessage(OpenedFile file, string textToRead)
    {
        var openedFile = SkEditorAPI.Files.GetOpenedFiles().Find(f => f == file);
        var result = await SkEditorAPI.Windows.ShowDialog(
            Translation.Get("Attention"),
            Translation.Get("ChangesDetected"),
            new SymbolIconSource { Symbol = Symbol.ImportantFilled },
            primaryButtonText: "Yes",
            cancelButtonText: "No");


        if (result == ContentDialogResult.Primary)
        {
            openedFile.Editor.Document.Text = textToRead;
            SetLastKnownContent(SkEditorAPI.Files.GetCurrentOpenedFile(), textToRead);
            openedFile.IsSaved = true;
        }
        else
        {
            SetLastKnownContent(openedFile, textToRead);
            openedFile.IsSaved = false;
        }
    }
}
