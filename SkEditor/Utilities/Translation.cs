using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SkEditor.API;
using System;
using System.Collections.Generic;
using System.IO;

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

    public async static void LoadDefaultLanguage()
    {
        Uri languageXaml = new(Path.Combine(LanguagesFolder, $"English.xaml"));
        await using var stream = new FileStream(languageXaml.OriginalString, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 16384, FileOptions.Asynchronous);
        if (AvaloniaRuntimeXamlLoader.Load(stream) is ResourceDictionary dictionary)
        {
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
        }
    }

    private static HashSet<ResourceDictionary> _translations = [];

    public async static void ChangeLanguage(string language)
    {
        foreach (ResourceDictionary translation in _translations)
        {
            Application.Current.Resources.MergedDictionaries.Remove(translation);
        }

        Uri languageXaml = new(Path.Combine(LanguagesFolder, $"{language}.xaml"));

        if (!File.Exists(languageXaml.OriginalString))
        {
            ApiVault.Get().GetAppConfig().Language = "English";
            ApiVault.Get().GetAppConfig().Save();
            LoadDefaultLanguage();
            return;
        }

        await using var languageStream = new FileStream(languageXaml.OriginalString, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 16384, FileOptions.Asynchronous);

        if (AvaloniaRuntimeXamlLoader.Load(languageStream) is ResourceDictionary dictionary)
        {
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
            _translations.Add(dictionary);
        }
    }
}
