using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using SkEditor.API;
using SkEditor.API.Settings;

namespace SkEditor.Utilities.InternalAPI;

/// <summary>
///     Manager for custom addon settings.
/// </summary>
public static class AddonSettingsManager
{
    private static readonly Dictionary<IAddon, JsonObject> LoadedAddonSettings = new();
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void LoadSettings(IAddon addon)
    {
        if (LoadedAddonSettings.ContainsKey(addon))
        {
            return;
        }

        string path = Path.Combine(SkEditorAPI.Configs.GetAppDataPath(), "Addons", addon.Identifier, "settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (!File.Exists(path))
        {
            LoadedAddonSettings[addon] = CreateDefaultSettings(addon);
            return;
        }

        string json = File.ReadAllText(path);
        try
        {
            JsonNode? node = JsonNode.Parse(json);
            LoadedAddonSettings[addon] = node?.AsObject() ?? new JsonObject();
        }
        catch (Exception e)
        {
            SkEditorAPI.Logs.Error($"Failed to parse settings for addon {addon.Identifier}: {e.Message}");
            LoadedAddonSettings[addon] = CreateDefaultSettings(addon);
            return;
        }

        foreach (Setting setting in addon.GetSettings()
                     .Where(setting =>
                         !LoadedAddonSettings[addon].ContainsKey(setting.Key) && !setting.Type.IsSelfManaged))
        {
            LoadedAddonSettings[addon][setting.Key] = setting.Type.Serialize(setting.DefaultValue);
        }
    }

    private static JsonObject CreateDefaultSettings(IAddon addon)
    {
        JsonObject obj = new();
        foreach (Setting setting in addon.GetSettings().Where(s => !s.Type.IsSelfManaged))
        {
            obj[setting.Key] = setting.Type.Serialize(setting.DefaultValue);
        }

        return obj;
    }

    private static void SaveSettings(IAddon addon)
    {
        if (!LoadedAddonSettings.TryGetValue(addon, out JsonObject? addonSettings))
        {
            return;
        }

        string path = Path.Combine(SkEditorAPI.Configs.GetAppDataPath(), "Addons", addon.Identifier, "settings.json");

        string json = addonSettings.ToJsonString(JsonOptions);
        File.WriteAllText(path, json);
    }

    public static object? GetValue(Setting setting)
    {
        LoadSettings(setting.Addon);

        if (!LoadedAddonSettings[setting.Addon].TryGetPropertyValue(setting.Key, out JsonNode? value) || value == null)
        {
            return setting.DefaultValue;
        }

        return setting.Type.Deserialize(value);
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