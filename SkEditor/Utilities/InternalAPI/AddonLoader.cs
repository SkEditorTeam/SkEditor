using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json.Linq;
using SkEditor.API;
using SkEditor.Utilities.InternalAPI.Classes;
using SkEditor.Views;
using SkEditor.Views.Settings;

namespace SkEditor.Utilities.InternalAPI;

/// <summary>
/// This class should not be used directly. It's for internal use only.
/// If you want to manage addons, use <see cref="SkEditorAPI.Addons"/> instead.
/// </summary>
public static class AddonLoader
{
    public static List<AddonMeta> Addons { get; } = [];
    public static HashSet<string> DllNames { get; } = [];
    private static JObject RawMeta = null!;

    public static void Load()
    {
        Directory.CreateDirectory(Path.Combine(AppConfig.AppDataFolderPath, "Addons"));
        
        Addons.Clear();
        LoadMeta();
        LoadAddon(typeof(SkEditorSelfAddon));
        LoadAddonsFromFiles();
        
        CheckForAddonsErrors();
    }

    private static async void CheckForAddonsErrors()
    {
        var addonsWithErrors = Addons.Where(addon => addon.HasErrors).ToList();
        if (addonsWithErrors.Count == 0) 
            return;

        var response = await SkEditorAPI.Windows.ShowDialog(
            "Addons with errors", $"Some addons ({addonsWithErrors.Count}) have errors. Do you want to see them?",
            Symbol.AlertUrgent, "Cancel");

        if (response == ContentDialogResult.Primary)
        {
            var window = new SettingsWindow();
            SettingsWindow.NavigateToPage(typeof(AddonsPage));
            await window.ShowDialog(SkEditorAPI.Windows.GetMainWindow());
        }
    }

    private static void LoadMeta()
    {
        var metaFile = Path.Combine(AppConfig.AppDataFolderPath, "Addons", "meta.json");
        if (!File.Exists(metaFile)) 
            File.WriteAllText(metaFile, "{}");
        
        RawMeta = JObject.Parse(File.ReadAllText(metaFile));
    }

    private static async void LoadAddonsFromFiles()
    {
        var folder = Path.Combine(AppConfig.AppDataFolderPath, "Addons");
        var folders = Directory.GetDirectories(folder);
        var dllFiles = new List<string>();
        foreach (string sub in folders)
        {
            var dllFile = Path.Combine(sub, Path.GetFileName(sub) + ".dll");
            if (!File.Exists(dllFile))
                continue;
            
            dllFiles.Add(dllFile);
        }
        SkEditorAPI.Logs.Debug($"Found {dllFiles.Count} addon dll files.");

        foreach (var dllFile in dllFiles)
            await LoadAddonFromFile(Path.GetDirectoryName(dllFile));
    }

    public static async Task LoadAddonFromFile(string addonFolder)
    {
        var dllFile = Path.Combine(addonFolder, Path.GetFileName(addonFolder) + ".dll");
        if (!File.Exists(dllFile))
        {
            SkEditorAPI.Logs.Error($"Failed to load addon from \"{addonFolder}\": No dll file found.");
            return;
        }

        AddonLoadContext loadContext = new AddonLoadContext(Path.GetFullPath(dllFile));
        List<IAddon?> addon;
        try
        {
            var stream = File.OpenRead(dllFile);
            
            addon = loadContext.LoadFromStream(stream)
                .GetTypes()
                .Where(p => typeof(IAddon).IsAssignableFrom(p) && p is { IsClass: true, IsAbstract: false })
                .Select(addonType => (IAddon)Activator.CreateInstance(addonType))
                .ToList();
            
            stream.Close();
        }
        catch (Exception e)
        {
            SkEditorAPI.Logs.Warning($"Failed to load addon from \"{dllFile}\": {e.Message}, maybe it's the wrong architecture?");
            return;
        }

        if (addon.Count == 0)
        {
            SkEditorAPI.Logs.Warning($"Failed to load addon from \"{dllFile}\": No addon class found. No worries if it's a library.");
            return;
        }
        
        if (addon.Count > 1)
        {
            SkEditorAPI.Logs.Warning($"Failed to load addon from \"{dllFile}\": Multiple addon classes found.");
            return;
        }
        
        if (addon[0] is SkEditorSelfAddon)
        {
            SkEditorAPI.Logs.Warning($"Failed to load addon from \"{dllFile}\": The SkEditor Core can't be loaded as an addon.");
            return;
        }
        
        if (Addons.Any(m => m.Addon.Identifier == addon[0].Identifier))
        {
            SkEditorAPI.Logs.Warning($"Failed to load addon from \"{dllFile}\": An addon with the identifier \"{addon[0].Identifier}\" is already loaded.");
            return;
        }
        
        Addons.Add(new AddonMeta()
        {
            Addon = addon[0],
            State = IAddons.AddonState.Installed,
            DllFilePath = dllFile,
            Errors = [],
            LoadContext = loadContext
        });

        await EnableAddon(addon[0]);
    }

    public static async void LoadAddon(Type addonClass)
    {
        var addon = (IAddon) Activator.CreateInstance(addonClass);
        Addons.Add(new AddonMeta()
        {
            Addon = addon, State = IAddons.AddonState.Disabled,
            Errors = []
        });

        await EnableAddon(addon);
    }

    private static bool CanEnable(AddonMeta addonMeta)
    {
        var addon = addonMeta.Addon;
        var minimalVersion = addon.GetMinimalSkEditorVersion();
        if (SkEditorAPI.Core.GetAppVersion() < minimalVersion)
        {
            SkEditorAPI.Logs.Debug($"Addon \"{addon.Name}\" requires SkEditor version {minimalVersion}, but the current version is {SkEditorAPI.Core.GetAppVersion()}. Disabling it.");
            addonMeta.State = IAddons.AddonState.Disabled;
            addonMeta.Errors.Add(LoadingErrors.OutdatedSkEditor(minimalVersion));
            return false;
        }
                
        var maximalVersion = addon.GetMaximalSkEditorVersion();
        if (maximalVersion != null && SkEditorAPI.Core.GetAppVersion().CompareTo(maximalVersion) > 0)
        {
            SkEditorAPI.Logs.Debug($"Addon \"{addon.Name}\" requires SkEditor version {maximalVersion}, but the current version is {SkEditorAPI.Core.GetAppVersion()}. Disabling it.");
            addonMeta.State = IAddons.AddonState.Disabled;
            addonMeta.Errors.Add(LoadingErrors.OutdatedAddon(maximalVersion));
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Preload an addon. This will make the desired addon ready to be enabled,
    /// by checking if it can actually be enabled, and its dependencies.
    /// </summary>
    /// <param name="addonMeta">The addon to preload.</param>
    /// <returns>True if the addon can be enabled, false otherwise.</returns>
    public static async Task<bool> PreLoad(AddonMeta addonMeta)
    {
        if (!CanEnable(addonMeta))
            return false;

        if (!await LocalDependencyManager.CheckAddonDependencies(addonMeta))
            return false;

        if (addonMeta.NeedsRestart)
        {
            await SkEditorAPI.Windows.ShowMessage("Addon needs restart", $"The addon \"{addonMeta.Addon.Name}\" needs a restart to be enabled correctly.");
            addonMeta.State = IAddons.AddonState.Disabled;
            return false;
        }
        
        return true;
    }

    public static async Task<bool> EnableAddon(IAddon addon)
    {
        var meta = Addons.First(m => m.Addon == addon);
        if (meta.State == IAddons.AddonState.Enabled) 
            return true;
        
        if (!await PreLoad(meta))
            return false;
        
        try
        {
            AddonSettingsManager.LoadSettings(addon);
            
            addon.OnEnable();
            await addon.OnEnableAsync();

            meta.State = IAddons.AddonState.Enabled;
            SaveMeta();
            SkEditorAPI.Windows.GetMainWindow().ReloadUiOfAddons();
            return true;
        }
        catch (Exception e)
        {
            SkEditorAPI.Logs.Error($"Failed to enable addon \"{addon.Name}\": {e.Message}");
            SkEditorAPI.Logs.Fatal(e);
            meta.Errors.Add(LoadingErrors.LoadingException(e));
            meta.State = IAddons.AddonState.Disabled;
            SaveMeta();
            Registries.Unload(addon);
            SkEditorAPI.Windows.GetMainWindow().ReloadUiOfAddons();
            return false;
        }
    }
    
    public static void DisableAddon(IAddon addon)
    {
        var meta = Addons.First(m => m.Addon == addon);
        if (meta.State == IAddons.AddonState.Disabled) 
            return;

        try
        {
            addon.OnDisable();
            meta.State = IAddons.AddonState.Disabled;
        }
        catch (Exception e)
        {
            SkEditorAPI.Logs.Error($"Failed to disable addon \"{addon.Name}\": {e.Message}");
            meta.State = IAddons.AddonState.Disabled;
        }

        SaveMeta();
        Registries.Unload(addon);
        SkEditorAPI.Windows.GetMainWindow().ReloadUiOfAddons();
    }
    
    public static bool IsAddonEnabled(IAddon addon)
    {
        return Addons.First(m => m.Addon == addon).State == IAddons.AddonState.Enabled;
    }

    public static void DeleteAddon(IAddon addon)
    {
        if (addon is SkEditorSelfAddon)
        {
            SkEditorAPI.Logs.Error("You can't delete the SkEditor Core.", true);
            return;
        }
        
        var addonMeta = Addons.First(m => m.Addon == addon);
        if (addonMeta.State == IAddons.AddonState.Enabled)
        {
            try
            {
                addon.OnDisable();
            }
            catch (Exception e)
            {
                SkEditorAPI.Logs.Error($"Failed to disable addon \"{addon.Name}\": {e.Message}");
                Registries.Unload(addon);
            }
        }
        
        addonMeta.LoadContext.Unload();
        
        var addonFile = Path.Combine(AppConfig.AppDataFolderPath, "Addons", addonMeta.Addon.Identifier, addonMeta.Addon.Identifier + ".dll");
        if (File.Exists(addonFile))
            File.Delete(addonFile);

        Addons.Remove(addonMeta);
        SaveMeta();
        Registries.Unload(addon);
        SkEditorAPI.Windows.GetMainWindow().ReloadUiOfAddons();
    }

    public static IAddon? GetAddonByNamespace(string? addonNamespace)
    {
        return Addons.FirstOrDefault(addon => addon.Addon.GetType().Namespace == addonNamespace)?.Addon;
    }

    public static SkEditorSelfAddon GetCoreAddon()
    {
        return (SkEditorSelfAddon) Addons.First(addon => addon.Addon is SkEditorSelfAddon).Addon;
    }

    public static async void SaveMeta()
    {
        var metaFile = Path.Combine(AppConfig.AppDataFolderPath, "Addons", "meta.json");
        var objs = new JObject();
        foreach (var addonMeta in Addons)
        {
            objs[addonMeta.Addon.Identifier] = addonMeta.State == IAddons.AddonState.Enabled;
        }
        await File.WriteAllTextAsync(metaFile, objs.ToString());
    }

    public static IAddons.AddonState GetAddonState(IAddon addon)
    {
        return Addons.First(m => m.Addon == addon).State;
    }

    public static void HandleAddonMethod(Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            SkEditorAPI.Logs.Error($"Failed to execute addon method: {e.Message}", true);
            SkEditorAPI.Logs.Fatal(e);
        }
    }
}
