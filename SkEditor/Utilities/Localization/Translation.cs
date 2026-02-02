using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using Avalonia.Threading;
using SkEditor.API;
using SkEditor.Utilities.Extensions;

namespace SkEditor.Utilities.Localization;

/// <summary>
///     Provides functionality for translating strings based on the current language setting.
/// </summary>
public static class Translation
{
    private const string DefaultLanguage = "English";
    private static readonly Dictionary<string, ResourceDictionary> Translations = [];

    /// <summary>
    ///     The folder where language files are stored.
    /// </summary>
    public static string LanguagesFolder { get; } = Path.Combine(AppContext.BaseDirectory, "Languages");

    /// <summary>
    ///     Retrieves the translated string for the given key, formatting it with the provided parameters if any.
    /// </summary>
    /// <param name="key">The translation key.</param>
    /// <param name="parameters">Optional parameters to format the translated string.</param>
    /// <returns>The translated and formatted string.</returns>
    public static string Get(string key, params string?[] parameters)
    {
        object? translation = null;
        Application.Current?.TryGetResource(key, ThemeVariant.Default, out translation);
        string translationString = translation?.ToString() ?? key;
        translationString = translationString.Replace("\\n", Environment.NewLine);

        if (parameters.Length <= 0)
        {
            return translationString;
        }

        try
        {
            return string.Format(translationString, parameters);
        }
        catch (FormatException)
        {
            return translationString;
        }
    }
    
    public static TranslatableString GetObservable(string key)
    {
        return new TranslatableString(key);
    }

    /// <summary>
    ///     Changes the application's language to the specified language.
    /// </summary>
    /// <param name="language">The language to change to.</param>
    /// <param name="startup">Whether this change is occurring during application startup.</param>
    public static async Task ChangeLanguage(string language, bool startup = false)
    {
        ClearOldLanguages();

        ResourceDictionary newLanguageDict = await CreateLanguageDictionary(language);

        AddDictionary(language, newLanguageDict);

        if (!startup)
        {
            PropagateLanguageChange(language);
        }
    }

    private static void ClearOldLanguages()
    {
        foreach (KeyValuePair<string, ResourceDictionary> translation in Translations
                     .Where(t => t.Key != DefaultLanguage)
                     .ToList())
        {
            Application.Current?.Resources.MergedDictionaries.Remove(translation.Value);
            Translations.Remove(translation.Key);
        }
    }

    private static async Task<ResourceDictionary> CreateLanguageDictionary(string language)
    {
        ResourceDictionary languageDict = new();

        bool isDefault = language == DefaultLanguage;
        bool isDefaultLoaded = Application.Current?.Resources.MergedDictionaries
                                   .Any(d => Translations.TryGetValue(DefaultLanguage, out ResourceDictionary? dict) &&
                                             Equals(d, dict)) ??
                               false;

        if (!isDefaultLoaded)
        {
            await LoadDefault(languageDict);
        }

        if (!isDefault)
        {
            await LoadLanguage(language, languageDict);
        }

        return languageDict;
    }

    private static async Task LoadDefault(ResourceDictionary newLanguageDict)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        const string resourceName = $"SkEditor.Languages.{DefaultLanguage}.json";

        await using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
        using MemoryStream ms = new();
        await stream.CopyToAsync(ms);
        LoadToResourceDictionary(ms.ToArray(), newLanguageDict);
    }

    private static async Task LoadLanguage(string language, ResourceDictionary newLanguageDict)
    {
        string languageFile = Path.Combine(LanguagesFolder, $"{language}.json").NormalizePathSeparators();

        if (!File.Exists(languageFile))
        {
            return;
        }

        byte[] bytes = await File.ReadAllBytesAsync(languageFile);
        LoadToResourceDictionary(bytes, newLanguageDict);
    }

    private static void LoadToResourceDictionary(ReadOnlySpan<byte> jsonUtf8, ResourceDictionary dict)
    {
        Utf8JsonReader reader = new(jsonUtf8);

        reader.Read();

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of JSON object");
        }

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            string key = reader.GetString()!;
            reader.Read();
            string value = reader.GetString()!;

            dict[key] = value;
        }
    }

    private static void AddDictionary(string language, ResourceDictionary newLanguageDict)
    {
        Application.Current?.Resources.MergedDictionaries.Add(newLanguageDict);
        Translations.TryAdd(language, newLanguageDict);
    }

    private static void PropagateLanguageChange(string language)
    {
        SkEditorAPI.Configs.GetAppSettings().Language = language;
        _ = SkEditorAPI.Configs.GetAppSettings().Save();
        Dispatcher.UIThread.Post(() => SkEditorAPI.Events.LanguageChanged(language));
    }
}