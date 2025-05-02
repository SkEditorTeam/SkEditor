using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.IO;
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

    private static readonly HashSet<ResourceDictionary> Translations = [];

    public static async Task ChangeLanguage(string language)
    {
        foreach (ResourceDictionary translation in Translations)
        {
            Application.Current.Resources.MergedDictionaries.Remove(translation);
        }

        Uri languageXaml = new(Path.Combine(LanguagesFolder, $"{language}.xaml"));

        if (!File.Exists(languageXaml.OriginalString))
        {
            SkEditorAPI.Core.GetAppConfig().Language = "English";
            SkEditorAPI.Core.GetAppConfig().Save();
            return;
        }

        await using var languageStream = new FileStream(languageXaml.OriginalString, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 16384, FileOptions.Asynchronous);

        if (AvaloniaRuntimeXamlLoader.Load(languageStream) is ResourceDictionary dictionary)
        {
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
            Translations.Add(dictionary);
        }
    }
}
