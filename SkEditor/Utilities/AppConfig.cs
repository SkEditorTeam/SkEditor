using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SkEditor.Utilities;


public class AppConfig
{
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
    public Dictionary<string, string> FileSyntaxes { get; set; } = new();
    public string Font { get; set; } = "Default";
    public bool CheckForUpdates { get; set; } = true;
    public bool CheckForChanges { get; set; } = true;

    public HashSet<string> AddonsToDisable { get; set; } = [];
    public HashSet<string> AddonsToDelete { get; set; } = [];
    public HashSet<string> AddonsToUpdate { get; set; } = [];

    public Dictionary<string, object> CustomOptions { get; set; } = [];

    public bool EnableAutoCompletionExperiment { get; set; } = false;

    public bool UseSpacesInsteadOfTabs { get; set; } = false;
    public int TabSize { get; set; } = 4;
    public bool EnableProjectsExperiment { get; set; } = false;
    public bool EnableHexPreview { get; set; } = false;


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

    public void SetUpNewOption(string optionName, object defaultValue)
    {
        if (!CustomOptions.ContainsKey(optionName))
        {
            CustomOptions[optionName] = defaultValue;
        }
    }

    public void SetOption(string optionName, object value)
    {
        if (CustomOptions.ContainsKey(optionName))
        {
            CustomOptions[optionName] = value;
        }
    }

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
}