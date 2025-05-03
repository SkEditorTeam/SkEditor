using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkEditor.Utilities;

public static class Translation
{
    public static string LanguagesFolder { get; } = $"{AppContext.BaseDirectory}/Languages";

    public static string Get(string key, params string[] parameters)
    {
        Application.Current.TryGetResource(key, Avalonia.Styling.ThemeVariant.Default, out object translation);
        string translationString = translation?.ToString() ?? key;
        translationString = translationString.Replace("\\n", Environment.NewLine);

        for (int i = 0; i < parameters.Length; i++)
        {
            translationString = translationString.Replace($"{{{i}}}", parameters[i]);
        }

        return translationString;
    }

    private static readonly Dictionary<string, ResourceDictionary> Translations = [];

    public static async Task ChangeLanguage(string language)
    {
#if !AOT
        foreach (KeyValuePair<string, ResourceDictionary> translation in Translations.Where(translation =>
                     translation.Key != "English"))
        {
            Application.Current.Resources.MergedDictionaries.Remove(translation.Value);
            Translations.Remove(translation.Key);
        }

        Uri languageXaml = new(Path.Combine(LanguagesFolder, $"{language}.xaml"));

        if (!File.Exists(languageXaml.OriginalString))
        {
            SkEditorAPI.Core.GetAppConfig().Language = "English";
            SkEditorAPI.Core.GetAppConfig().Save();
            return;
        }

        await using var languageStream = new FileStream(languageXaml.OriginalString, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite, 16384, FileOptions.Asynchronous);

        if (AvaloniaRuntimeXamlLoader.Load(languageStream) is ResourceDictionary dictionary)
        {
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
            Translations.Add(language, dictionary);
        }
#endif
    }
}