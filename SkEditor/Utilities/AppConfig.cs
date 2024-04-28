using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SkEditor.Utilities;


public class AppConfig
{
    public string Version { get; set; } = string.Empty;
    public bool FirstTime { get; set; } = true;
    public string Language { get; set; } = "English";
    public string LastUsedPublishService { get; set; } = "Pastebin";
    public string PastebinApiKey { get; set; } = "";
    public string CodeSkriptPlApiKey { get; set; } = "";
    public string SkunityApiKey { get; set; } = "";
    public bool UseSkriptGui { get; set; } = false;

    public bool IsDiscordRpcEnabled { get; set; } = true;
    public bool IsWrappingEnabled { get; set; } = false;
    public bool IsAutoIndentEnabled { get; set; } = false;
    public bool IsAutoPairingEnabled { get; set; } = false;
    public bool IsAutoSaveEnabled { get; set; } = false;
    public string CurrentTheme { get; set; } = "Default.json";
    public Dictionary<string, string> FileSyntaxes { get; set; } = [];
    public string Font { get; set; } = "Default";
    public bool UseSpacesInsteadOfTabs { get; set; } = false;
    public int TabSize { get; set; } = 4;
    public bool CheckForUpdates { get; set; } = true;
    public bool CheckForChanges { get; set; } = true;

    public HashSet<string> AddonsToDisable { get; set; } = [];
    public HashSet<string> AddonsToDelete { get; set; } = [];
    public HashSet<string> AddonsToUpdate { get; set; } = [];

    public Dictionary<string, object> CustomOptions { get; set; } = [];
    public Dictionary<string, string> PreferredFileAssociations { get; set; } = [];

    public bool EnableAutoCompletionExperiment { get; set; } = false;
    public bool EnableProjectsExperiment { get; set; } = false;
    public bool EnableHexPreview { get; set; } = false;
    public bool EnableCodeParser { get; set; } = false;
    public bool EnableFolding { get; set; } = false;
    public bool EnableBetterPairing { get; set; } = false;
    public bool EnableSessionRestoring { get; set; } = false;


    public static string AppDataFolderPath { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor");

    public static string SettingsFilePath { get; set; } = Path.Combine(AppDataFolderPath, "settings.json");

    public static async Task<AppConfig> Load()
    {
        string settingsFilePath = SettingsFilePath;

        if (!File.Exists(settingsFilePath) || string.IsNullOrWhiteSpace(File.ReadAllText(settingsFilePath)))
        {
            return LoadDefaultSettings();
        }

        try
        {
            return JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(settingsFilePath)) ?? new AppConfig();
        }
        catch (JsonException)
        {
            return LoadDefaultSettings();
        }
    }

    public void Save()
    {
        string jsonContent = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(SettingsFilePath, jsonContent);
    }

    public static AppConfig LoadDefaultSettings()
    {
        if (!Directory.Exists(AppDataFolderPath)) Directory.CreateDirectory(AppDataFolderPath);

        AppConfig defaultSettings = new();

        string jsonContent = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
        File.WriteAllText(SettingsFilePath, jsonContent);

        return defaultSettings;
    }

    /// <summary>
    /// Set up a new custom option.
    /// </summary>
    public void SetUpNewOption(string optionName, object defaultValue)
    {
        if (!CustomOptions.ContainsKey(optionName))
        {
            CustomOptions[optionName] = defaultValue;
        }
    }

    /// <summary>
    /// Set value of a custom option.
    /// </summary>
    public void SetOption(string optionName, object value)
    {
        if (CustomOptions.ContainsKey(optionName))
        {
            CustomOptions[optionName] = value;
        }
    }

    /// <summary>
    /// Get value of a custom option.
    /// </summary>
    public object GetOption(string optionName)
    {
        if (CustomOptions.TryGetValue(optionName, out object? value))
        {
            return value;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Get value of an option by name.
    /// </summary>
    public T GetOptionValue<T>(string optionName)
    {
        return (T)GetType().GetProperty(optionName).GetValue(this);
    }

    /// <summary>
    /// Set value of an option by name.
    /// </summary> 
    public void SetOptionValue<T>(string optionName, T value)
    {
        GetType().GetProperty(optionName).SetValue(this, value);
    }
}