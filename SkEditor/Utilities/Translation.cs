using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using SkEditor.API;
using SkEditor.Utilities.Extensions;

namespace SkEditor.Utilities;

public static class Translation
{
    private static readonly Dictionary<string, ResourceDictionary> Translations = [];
    public static string LanguagesFolder { get; } = Path.Combine(AppContext.BaseDirectory, "Languages");

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

    public static async Task ChangeLanguage(string language, bool startup = false)
    {
        bool isEnglish = language == "English";

        if (startup && isEnglish)
        {
            // English is a built-in ResourceDictionary, so we don't need to load it at startup.
            return;
        }

        foreach (KeyValuePair<string, ResourceDictionary> translation in Translations.Where(translation =>
                     translation.Key != "English").ToList())
        {
            Application.Current?.Resources.MergedDictionaries.Remove(translation.Value);
            Translations.Remove(translation.Key);
        }

        if (isEnglish)
        {
            SkEditorAPI.Core.GetAppConfig().Language = "English";
            SkEditorAPI.Core.GetAppConfig().Save();
            Dispatcher.UIThread.Post(() => SkEditorAPI.Events.LanguageChanged("English"));
            return;
        }

        string languageFile = Path.Combine(LanguagesFolder, $"{language}.json").NormalizePathSeparators();

        if (!File.Exists(languageFile))
        {
            SkEditorAPI.Core.GetAppConfig().Language = "English";
            SkEditorAPI.Core.GetAppConfig().Save();
            Dispatcher.UIThread.Post(() => SkEditorAPI.Events.LanguageChanged("English"));
            return;
        }

        byte[] bytes = await File.ReadAllBytesAsync(languageFile);

        ResourceDictionary languageDictionary = new();
        LoadToResourceDictionary(bytes, languageDictionary);

        Application.Current?.Resources.MergedDictionaries.Add(languageDictionary);
        Translations.TryAdd(language, languageDictionary);

        SkEditorAPI.Core.GetAppConfig().Language = language;
        SkEditorAPI.Core.GetAppConfig().Save();
        Dispatcher.UIThread.Post(() => SkEditorAPI.Events.LanguageChanged(language));
    }

    public static void LoadToResourceDictionary(ReadOnlySpan<byte> jsonUtf8, ResourceDictionary dict)
    {
        Utf8JsonReader reader = new(jsonUtf8);

        reader.Read();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            string key = reader.GetString()!;
            reader.Read();
            string value = reader.GetString()!;

            dict[key] = value;
        }
    }
}