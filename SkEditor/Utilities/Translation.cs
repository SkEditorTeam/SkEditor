using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using SkEditor.API;

namespace SkEditor.Utilities;

public static class Translation
{
    private static readonly Dictionary<string, ResourceDictionary> Translations = [];
    public static string LanguagesFolder { get; } = $"{AppContext.BaseDirectory}/Languages";

    public static string Get(string key, params string?[] parameters)
    {
        object? translation = null;
        Application.Current?.TryGetResource(key, ThemeVariant.Default, out translation);
        string translationString = translation?.ToString() ?? key;
        translationString = translationString.Replace("\\n", Environment.NewLine);

        for (int i = 0; i < parameters.Length; i++)
        {
            translationString = translationString.Replace($"{{{i}}}", parameters[i]);
        }

        return translationString;
    }

    public static async Task ChangeLanguage(string language)
    {
#if !AOT
        foreach (KeyValuePair<string, ResourceDictionary> translation in Translations.Where(translation =>
                     translation.Key != "English"))
        {
            Application.Current?.Resources.MergedDictionaries.Remove(translation.Value);
            Translations.Remove(translation.Key);
        }

        Uri languageXaml = new(Path.Combine(LanguagesFolder, $"{language}.xaml"));

        if (!File.Exists(languageXaml.OriginalString))
        {
            SkEditorAPI.Core.GetAppConfig().Language = "English";
            SkEditorAPI.Core.GetAppConfig().Save();
            Dispatcher.UIThread.Post(() => SkEditorAPI.Events.LanguageChanged("English"));
            return;
        }

        await using FileStream languageStream = new(languageXaml.OriginalString, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite, 16384, FileOptions.Asynchronous);

        if (AvaloniaRuntimeXamlLoader.Load(languageStream) is ResourceDictionary dictionary)
        {
            Application.Current?.Resources.MergedDictionaries.Add(dictionary);
            Translations.TryAdd(language, dictionary);
        }

        SkEditorAPI.Core.GetAppConfig().Language = language;
        SkEditorAPI.Core.GetAppConfig().Save();
        Dispatcher.UIThread.Post(() => SkEditorAPI.Events.LanguageChanged(language));
#endif
    }
}