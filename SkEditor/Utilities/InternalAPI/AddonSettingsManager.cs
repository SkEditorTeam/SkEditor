using Newtonsoft.Json.Linq;
using SkEditor.API;
using SkEditor.API.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkEditor.Utilities.InternalAPI;

/// <summary>
/// Manager for custom addon settings.
/// </summary>
public static class AddonSettingsManager
{

    private static readonly Dictionary<IAddon, JObject> LoadedAddonSettings = new();

    public static void LoadSettings(IAddon addon)
    {
        if (LoadedAddonSettings.ContainsKey(addon))
            return;

        var path = Path.Combine(AppConfig.AppDataFolderPath, "Addons", addon.Identifier, "settings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        if (!File.Exists(path))
        {
            LoadedAddonSettings[addon] = CreateDefaultSettings(addon);
            return;
        }

        var json = File.ReadAllText(path);
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

        foreach (var setting in addon.GetSettings())
        {
            if (!LoadedAddonSettings[addon].ContainsKey(setting.Key) && !setting.Type.IsSelfManaged)
            {
                SkEditorAPI.Logs.Warning($"Setting {setting.Key} not found in settings for addon {addon.Identifier}, using default value.");
                LoadedAddonSettings[addon][setting.Key] = setting.Type.Serialize(setting.DefaultValue);
            }
        }
    }

    private static JObject CreateDefaultSettings(IAddon addon)
    {
        var obj = new JObject();
        addon.GetSettings()
            .Where(s => !s.Type.IsSelfManaged)
            .ToList()
            .ForEach(setting => obj[setting.Key] = setting.Type.Serialize(setting.DefaultValue));

        return obj;
    }

    private static void SaveSettings(IAddon addon)
    {
        if (!LoadedAddonSettings.TryGetValue(addon, out JObject? value))
            return;

        var path = Path.Combine(AppConfig.AppDataFolderPath, "Addons", addon.Identifier, "settings.json");
        var json = value.ToString();
        File.WriteAllText(path, json);
    }

    public static object GetValue(Setting setting)
    {
        LoadSettings(setting.Addon);
        return setting.Type.Deserialize(LoadedAddonSettings[setting.Addon][setting.Key]);
    }

    public static void SetValue(Setting setting, object value)
    {
        LoadSettings(setting.Addon);
        LoadedAddonSettings[setting.Addon][setting.Key] = setting.Type.Serialize(value);
        SaveSettings(setting.Addon);
    }

    public static void SetAddonValue(IAddon addon, string key, object value)
    {
        var setting = addon.GetSettings().Find(s => s.Key == key);
        if (setting == null)
        {
            SkEditorAPI.Logs.Error($"Setting {key} not found in settings for addon {addon.Identifier}");
            return;
        }

        SetValue(setting, value);
    }

    public static object GetAddonValue(IAddon addon, string key)
    {
        var setting = addon.GetSettings().Find(s => s.Key == key);
        if (setting == null)
        {
            SkEditorAPI.Logs.Error($"Setting {key} not found in settings for addon {addon.Identifier}");
            return null;
        }

        return GetValue(setting);
    }

}