using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Serilog;
using SkEditor.Utilities;
using SkEditor.Views;

namespace SkEditor.API;
public class AddonLoader
{
    public static List<IAddon> Addons { get; } = [];

    public static void Load()
    {
        var addonFolder = Path.Combine(AppConfig.AppDataFolderPath, "Addons");
        Directory.CreateDirectory(addonFolder);

        UpdateAddons(addonFolder);
        DeleteDisableAddons(addonFolder);

        LoadAddonsFromFolder(addonFolder);
        EnableAddons();
    }

    private static void DeleteDisableAddons(string addonFolder)
    {
        IEnumerable<string> addonFolders = Directory.EnumerateDirectories(addonFolder, "*", SearchOption.TopDirectoryOnly);
        foreach (string folder in addonFolders)
        {
            if (ApiVault.Get().GetAppConfig().AddonsToDelete.Contains(Path.GetFileName(folder)))
            {
                Directory.Delete(folder, true);
                ApiVault.Get().GetAppConfig().AddonsToDisable.Remove(Path.GetFileName(folder));
                ApiVault.Get().GetAppConfig().AddonsToDelete.Remove(Path.GetFileName(folder));
                continue;
            }
            else if (ApiVault.Get().GetAppConfig().AddonsToDisable.Contains(Path.GetFileName(folder))) continue;

            var packagesFolder = Path.Combine(folder, "Packages");
            if (Directory.Exists(packagesFolder))
            {
                LoadAddonsFromFolder(packagesFolder);
            }
            LoadAddonsFromFolder(folder);
        }
    }

    private static void UpdateAddons(string addonFolder)
    {
        IEnumerable<string> updatedFolderAddons = Directory.EnumerateFiles(addonFolder, "*.zip", SearchOption.TopDirectoryOnly);

        foreach (string updatedAddon in updatedFolderAddons)
        {
            string nameWithoutPrefix = Path.GetFileName(updatedAddon)["updated-".Length..];
            string folderWithoutPrefixPath = Path.Combine(addonFolder, Path.GetFileNameWithoutExtension(nameWithoutPrefix));
            if (Directory.Exists(folderWithoutPrefixPath))
            {
                File.Delete(updatedAddon);
                continue;
            }

            Directory.Delete(folderWithoutPrefixPath, true);
            ZipFile.ExtractToDirectory(updatedAddon, addonFolder);
            File.Delete(updatedAddon);
        }
    }

    private static void EnableAddons()
    {
        try
        {
            AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IAddon).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .Select(addonType => (IAddon)Activator.CreateInstance(addonType))
                .ToList()
                .ForEach(addon =>
                {
                    Addons.Add(addon);
                    addon.OnEnable();
                });

            ApiVault.Get().GetAppConfig().AddonsToDelete.Clear();
            ApiVault.Get().GetAppConfig().AddonsToUpdate.Clear();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load addons");
        }
        
        MainWindow.Instance.MainMenu.LoadAddonsMenus();
    }

    public static List<Assembly> LoadAddonsFromFolder(string folder)
    {
        IEnumerable<string> dllFiles = Directory.EnumerateFiles(folder, "*.dll", SearchOption.TopDirectoryOnly);

        foreach (string updatedFile in dllFiles.Where(f => Path.GetFileName(f).StartsWith("updated-")))
        {
            string nameWithoutPrefix = Path.GetFileName(updatedFile)["updated-".Length..];
            string fileWithoutPrefixPath = Path.Combine(folder, nameWithoutPrefix);
            File.Delete(fileWithoutPrefixPath);
            File.Move(updatedFile, fileWithoutPrefixPath);
        }

        List<Assembly> assemblies = [];

        foreach (string dllFile in dllFiles)
        {
            string fileName = Path.GetFileName(dllFile);

            if (ApiVault.Get().GetAppConfig().AddonsToDelete.Contains(fileName))
            {
                File.Delete(dllFile);
                ApiVault.Get().GetAppConfig().AddonsToDisable.Remove(fileName);
                ApiVault.Get().GetAppConfig().AddonsToDelete.Remove(fileName);
                continue;
            }
            else if (ApiVault.Get().GetAppConfig().AddonsToDisable.Contains(fileName)) continue;

            if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == Path.GetFileNameWithoutExtension(dllFile))) continue;

            assemblies.Add(Assembly.LoadFrom(dllFile));
        }

        return assemblies;
    }
}
