using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using SkEditor.API;
using SkEditor.API.Settings;

namespace SkEditor.Utilities.InternalAPI;

/// <summary>
///     Manager for custom addon settings.
/// </summary>
public static class AddonSettingsManager
{
    private static readonly Dictionary<IAddon, JObject> LoadedAddonSettings = new();

    public static void LoadSettings(IAddon addon)
    {
        if (LoadedAddonSettings.ContainsKey(addon))
        {
            return;
        }

        string path = Path.Combine(AppConfig.AppDataFolderPath, "Addons", addon.Identifier, "settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (!File.Exists(path))
        {
            LoadedAddonSettings[addon] = CreateDefaultSettings(addon);
            return;
        }

        string json = File.ReadAllText(path);
        try
        {
            LoadedAddonSettings[addon] = JObject.Parse(json);
        }
        catch (Exception e)
        {
            SkEditorAPI.Logs.Error($"Failed to parse settings for addon {addon.Identifier}: {e.Message}");
            LoadedAddonSettings[addon] = CreateDefaultSettings(addon);
            return;
        }

        foreach (Setting setting in addon.GetSettings())
        {
            if (!LoadedAddonSettings[addon].ContainsKey(setting.Key) && !setting.Type.IsSelfManaged)
            {
                LoadedAddonSettings[addon][setting.Key] = setting.Type.Serialize(setting.DefaultValue);
            }
        }
    }

    private static JObject CreateDefaultSettings(IAddon addon)
    {
        JObject obj = new();
        addon.GetSettings()
            .Where(s => !s.Type.IsSelfManaged)
            .ToList()
            .ForEach(setting => obj[setting.Key] = setting.Type.Serialize(setting.DefaultValue));

        return obj;
    }

    private static void SaveSettings(IAddon addon)
    {
        if (!LoadedAddonSettings.TryGetValue(addon, out JObject? addonSettings))
        {
            return;
        }

        string path = Path.Combine(AppConfig.AppDataFolderPath, "Addons", addon.Identifier, "settings.json");

        string currentJson = File.ReadAllText(path);
        
        JObject currentSettings = JObject.Parse(currentJson);

        foreach (Setting setting in addon.GetSettings())
        {
            if (!setting.Type.IsSelfManaged && addonSettings.TryGetValue(setting.Key, out JToken? value))
            {
                currentSettings[setting.Key] = value;
            }
        }
        
        string json = currentSettings.ToString();
        File.WriteAllText(path, json);
    }

    public static object? GetValue(Setting setting)
    {
        LoadSettings(setting.Addon);
        JToken? addonSettings = LoadedAddonSettings[setting.Addon][setting.Key];
        if (addonSettings == null)
        {
            return setting.DefaultValue;
        }

        return addonSettings.Type != JTokenType.Null ? setting.Type.Deserialize(addonSettings) : setting.DefaultValue;
    }

    public static void SetValue(Setting setting, object value)
    {
        LoadSettings(setting.Addon);
        LoadedAddonSettings[setting.Addon][setting.Key] = setting.Type.Serialize(value);
        SaveSettings(setting.Addon);
    }

    public static void SetAddonValue(IAddon addon, string key, object value)
    {
        Setting? setting = addon.GetSettings().Find(s => s.Key == key);
        if (setting == null)
        {
            return;
        }

        SetValue(setting, value);
    }

    public static object? GetAddonValue(IAddon addon, string key)
    {
        Setting? setting = addon.GetSettings().Find(s => s.Key == key);
        return setting != null ? GetValue(setting) : null;
    }
}