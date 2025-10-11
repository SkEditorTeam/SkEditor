using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using Serilog;
using SkEditor.API;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Utilities.Files;

public class ChangeChecker
{
    private static readonly Dictionary<OpenedFile, string> LastKnownContentDictionary = [];

    private static bool _isMessageShown;
    public static Dictionary<string, bool> HasChangedDictionary { get; } = [];

    private static string GetLastKnownContent(OpenedFile openedFile)
    {
        return LastKnownContentDictionary.GetValueOrDefault(openedFile, "");
    }

    private static void SetLastKnownContent(OpenedFile openedFile, string content)
    {
        LastKnownContentDictionary[openedFile] = content;
    }

    public static async Task Check()
    {
        if (!SkEditorAPI.Core.GetAppConfig().CheckForChanges)
        {
            return;
        }

        try
        {
            OpenedFile? file = SkEditorAPI.Files.GetCurrentOpenedFile();
            if (file == null) return;

            if (!file.IsEditor || string.IsNullOrWhiteSpace(file.Path))
            {
                return;
            }

            string path = Uri.UnescapeDataString(file.Path);
            if (!File.Exists(path))
            {
                return;
            }

            if (!HasChangedDictionary.TryGetValue(path, out bool hasChanged))
            {
                HasChangedDictionary[path] = false;
                hasChanged = false;
            }

            if (hasChanged)
            {
                return;
            }

            string textToWrite = file.Editor?.Document?.Text ?? string.Empty;
            using StreamReader reader = new(path);
            string textToRead = await reader.ReadToEndAsync();

            if (textToWrite.Equals(textToRead) ||
                textToRead.Equals(GetLastKnownContent(file)))
            {
                return;
            }

            if (_isMessageShown)
            {
                return;
            }

            _isMessageShown = true;
            await ShowMessage(file, textToRead);
        }
        catch (Exception e)
        {
            Log.Warning(e, "Error while checking for changes");
        }

        _isMessageShown = false;
    }

    private static async Task ShowMessage(OpenedFile file, string textToRead)
    {
        OpenedFile? openedFile = SkEditorAPI.Files.GetOpenedFiles().Find(f => f == file);
        if (openedFile is not { Editor: not null }) return;
        
        ContentDialogResult result = await SkEditorAPI.Windows.ShowDialog(
            Translation.Get("Attention"),
            Translation.Get("ChangesDetected"),
            new SymbolIconSource { Symbol = Symbol.Important, IconVariant = IconVariant.Filled},
            primaryButtonText: "Yes",
            cancelButtonText: "No");

        if (result == ContentDialogResult.Primary)
        {
            openedFile.Editor.Document.Text = textToRead;
            SetLastKnownContent(openedFile, textToRead);
            openedFile.IsSaved = true;
        }
        else
        {
            SetLastKnownContent(openedFile, textToRead);
            openedFile.IsSaved = false;
        }
    }
}