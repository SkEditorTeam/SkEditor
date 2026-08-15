using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Models.Logging;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Services.Localization;

public class TranslationService : ITranslationService
{
    private const string DefaultLanguage = "English";
    private static readonly Dictionary<string, ResourceDictionary> Translations = [];
    private readonly AppSettings _appSettings;
    private readonly ILogService _logs;

    public static ITranslationService Current { get; set; } = null!;

    public string LanguagesFolder => Path.Combine(AppContext.BaseDirectory, "Languages");

    public TranslationService(ILogService logs, AppSettings appSettings)
    {
        _logs = logs;
        _appSettings = appSettings;

        appSettings.PropertyChanged += OnSettingsChanged;
    }

    public string Get(TranslationKey key, params string?[] parameters)
    {
        if (key == TranslationKey.None)
        {
            throw new ArgumentException("Translation key cannot be None");
        }

        object? translation = null;
        Application.Current?.TryGetResource(key.ToString(), ThemeVariant.Default, out translation);
        string translationString = translation?.ToString() ?? key.ToString();
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

    public TranslatableString GetObservable(TranslationKey key)
    {
        return new TranslatableString(key);
    }

    public TranslatableString GetObservable(TranslationKey key, params object[]? parameters)
    {
        return new TranslatableString(key, parameters);
    }

    public async Task ChangeLanguageAsync(string language, bool startup = false)
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

    private async Task<ResourceDictionary> CreateLanguageDictionary(string language)
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

    private async Task LoadLanguage(string language, ResourceDictionary newLanguageDict)
    {
        string languageFile = Path.Combine(LanguagesFolder, $"{language}.json").NormalizePathSeparators();

        if (!File.Exists(languageFile))
        {
            return;
        }

        byte[] bytes = await File.ReadAllBytesAsync(languageFile);
        try
        {
            LoadToResourceDictionary(bytes, newLanguageDict);
        }
        catch (JsonException ex)
        {
            _logs.Error($"Failed to load language file '{Path.GetFileName(languageFile)}': {ex.Message}",
                LogSource.SkEditor,
                true);
        }
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

    private void PropagateLanguageChange(string language)
    {
        LanguageChanged?.Invoke(this, language);
    }

    private void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppSettings.Language))
        {
            _ = ChangeLanguageAsync(_appSettings.Language);
        }
    }

    public event EventHandler<string>? LanguageChanged;
}