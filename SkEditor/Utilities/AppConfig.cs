using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using SkEditor.API;

namespace SkEditor.Utilities;

public partial class AppConfig : ObservableObject
{
    [ObservableProperty] private bool _checkForChanges;
    [ObservableProperty] private bool _checkForUpdates = true;
    [ObservableProperty] private string _codeSkriptPlApiKey = "";
    [ObservableProperty] private string _currentTheme = "Default.json";
    [ObservableProperty] private double _customUiScale = 1.0;
    [ObservableProperty] private Dictionary<string, string> _fileSyntaxes = [];
    [ObservableProperty] private bool _firstTime = true;
    [ObservableProperty] private string _font = "Default";
    [ObservableProperty] private bool _forceNativeTitleBar;
    [ObservableProperty] private bool _highlightCurrentLine = true;
    [ObservableProperty] private bool _isAutoIndentEnabled;
    [ObservableProperty] private bool _isAutoPairingEnabled;
    [ObservableProperty] private bool _isAutoSaveEnabled;
    [ObservableProperty] private bool _isDevModeEnabled;

    [ObservableProperty] private bool _isDiscordRpcEnabled = true;
    [ObservableProperty] private bool _isPasteIndentationEnabled;
    [ObservableProperty] private bool _isProjectSingleClickEnabled;
    [ObservableProperty] private bool _isSidebarAnimationEnabled;
    [ObservableProperty] private bool _isWrappingEnabled;
    [ObservableProperty] private bool _isZoomSyncEnabled;
    [ObservableProperty] private string _language = "English";
    [ObservableProperty] private string _lastUsedPublishService = "Pastebin";
    [ObservableProperty] private string _pastebinApiKey = "";
    [ObservableProperty] private int _tabSize = 4;
    [ObservableProperty] private bool _useSkriptGui;
    [ObservableProperty] private bool _useSpacesInsteadOfTabs;
    [ObservableProperty] private string _version = string.Empty;

    /// <summary>
    ///     Represent the width of panels via their ID (<see cref="Registries.SidebarPanels" />
    /// </summary>
    public Dictionary<string, int> SidebarPanelSizes { get; set; } = [];

    /// <summary>
    ///     Represent the (saved) choices the user made for file types.
    /// </summary>
    public Dictionary<string, string> FileTypeChoices { get; set; } = [];

    public Dictionary<string, object> CustomOptions { get; set; } = [];
    public Dictionary<string, string> PreferredFileAssociations { get; set; } = [];

    public bool EnableAutoCompletionExperiment { get; set; } = false;
    public bool EnableProjectsExperiment { get; set; } = false;
    public bool EnableHexPreview { get; set; } = false;
    public bool EnableCodeParser { get; set; }
    public bool EnableFolding { get; set; } = false;
    public bool EnableBetterPairing { get; set; } = false;
    public bool EnableSessionRestoring { get; set; } = false;
    public bool EnableRealtimeCodeParser { get; set; } = false;

    public string SkUnityApiKey { get; set; } = "";
    public string SkriptMcapiKey { get; set; } = "";

    public static string AppDataFolderPath { get; set; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SkEditor");

    public static string SettingsFilePath { get; set; } = Path.Combine(AppDataFolderPath, "settings.json");

    public static AppConfig Load()
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
        if (!Directory.Exists(AppDataFolderPath))
        {
            Directory.CreateDirectory(AppDataFolderPath);
        }

        AppConfig defaultSettings = new();

        string jsonContent = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
        File.WriteAllText(SettingsFilePath, jsonContent);

        return defaultSettings;
    }

    /// <summary>
    ///     Set up a new custom option.
    /// </summary>
    public void SetUpNewOption(string optionName, object defaultValue)
    {
        CustomOptions.TryAdd(optionName, defaultValue);
    }

    /// <summary>
    ///     Set value of a custom option.
    /// </summary>
    public void SetOption(string optionName, object value)
    {
        if (CustomOptions.ContainsKey(optionName))
        {
            CustomOptions[optionName] = value;
        }
    }

    /// <summary>
    ///     Get value of a custom option.
    /// </summary>
    public object GetOption(string optionName)
    {
        return CustomOptions.GetValueOrDefault(optionName);
    }

    /// <summary>
    ///     Get value of an option by name.
    /// </summary>
    public T GetOptionValue<T>(string optionName)
    {
        var property = GetType().GetProperty(optionName);
        if (property == null)
        {
            return default;
        }
        return (T)property.GetValue(this);
    }

    /// <summary>
    ///     Set value of an option by name.
    /// </summary>
    public void SetOptionValue<T>(string optionName, T value)
    {
        GetType().GetProperty(optionName).SetValue(this, value);
    }
}