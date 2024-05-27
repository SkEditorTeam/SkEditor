using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using SkEditor.API;
using SkEditor.Utilities.InternalAPI.Classes;

namespace SkEditor.Utilities.InternalAPI;

/// <summary>
/// Class to manage local dependencies, downloaded
/// from NuGet and store in the 'Dependencies' of 'Addons'.
/// </summary>
public static class LocalDependencyManager
{

    private static readonly Dictionary<string, NuGetVersion> _dependencies = new();
        
    public static void IndexDependencies()
    {
        _dependencies.Clear();
        
        var folder = Path.Combine(AppConfig.AppDataFolderPath, "Addons", "Dependencies");
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        
        var dllFiles = Directory.GetFiles(folder, "*.dll");
        foreach (var file in dllFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var version = NuGetVersion.Parse(fileName.Split('_')[1]);
            
            _dependencies.Add(fileName.Split('_')[0], version);
        }
    }

    public static async Task<bool> CheckAddonDependencies(AddonMeta addonMeta)
    {
        var nugetDependencies = addonMeta.Addon.GetDependencies()
            .Where(x => x is NuGetDependency)
            .Cast<NuGetDependency>().ToList();

        foreach (var dependency in nugetDependencies)
        {
            var dependencyName = dependency.PackageId;
            var dependencyVersion = dependency.Version == null ? null : new NuGetVersion(dependency.Version);
            NuGetVersion versionToLoad = null;
            
            // Check if the dependency is already downloaded
            if (_dependencies.TryGetValue(dependencyName, out NuGetVersion? value))
            {
                if (dependencyVersion != null && value < dependencyVersion)
                {
                    // Dependency is outdated
                    SkEditorAPI.Logs.Error($"Dependency {dependencyName} is outdated! Required: {dependencyVersion}, found: {value}");
                    addonMeta.Errors.Add(LoadingErrors.OutdatedDependency(dependencyName, 
                        dependencyVersion, value));
                    return false;
                }

                versionToLoad = value;
            }
            else
            {
                // Dependency is not downloaded
                SkEditorAPI.Logs.Debug($"Dependency {dependencyName} not found, downloading...");
                
                var cache = new SourceCacheContext();
                var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
                var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

                var versions = await resource.GetAllVersionsAsync(
                    dependencyName,
                    cache, NullLogger.Instance, default);

                var latestVersion = versions.OrderByDescending(x => x).First();
                if (dependencyVersion != null && latestVersion < dependencyVersion)
                {
                    SkEditorAPI.Logs.Error($"Dependency {dependencyName} is outdated! Required: {dependencyVersion}, found: {latestVersion}");
                    addonMeta.Errors.Add(LoadingErrors.OutdatedDependency(dependencyName, 
                        dependencyVersion, latestVersion));
                    return false;
                }
                
                if (dependencyVersion != null)
                    latestVersion = dependencyVersion;

                using var packageStream = new MemoryStream();
                await resource.CopyNupkgToStreamAsync(
                    dependencyName, latestVersion,
                    packageStream, cache,
                    NullLogger.Instance, default);

                var folder = Path.Combine(AppConfig.AppDataFolderPath, "Addons", "Dependencies");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                var filePath = Path.Combine(folder, $"{dependencyName}_{latestVersion}.dll");
                await File.WriteAllBytesAsync(filePath, packageStream.ToArray());
                
                _dependencies.Add(dependencyName, latestVersion);
                versionToLoad = latestVersion;
            }
            
            // don't forget to load the dependency to assembly
            var dependencyPath = Path.Combine(AppConfig.AppDataFolderPath, "Addons", "Dependencies", $"{dependencyName}_{versionToLoad}.dll");
            try
            {
                Assembly.LoadFile(dependencyPath);
            }
            catch (Exception e)
            {
                SkEditorAPI.Logs.Error($"Failed to load dependency {dependencyName} from {dependencyPath}: {e.Message}");
                addonMeta.Errors.Add(LoadingErrors.FailedToLoadDependency(dependencyName, e.Message));
                return false;
            }
        }
        
        return true;
    }

}