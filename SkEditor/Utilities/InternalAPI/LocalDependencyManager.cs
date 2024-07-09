using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using SkEditor.API;
using SkEditor.Utilities.InternalAPI.Classes;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileMode = System.IO.FileMode;
using Repository = NuGet.Protocol.Core.Types.Repository;

namespace SkEditor.Utilities.InternalAPI;

/// <summary>
/// Class to manage local dependencies, downloaded
/// from NuGet and store in the 'Dependencies' of 'Addons'.
/// </summary>
public static class LocalDependencyManager
{

    public static async Task<bool> CheckAddonDependencies(AddonMeta addonMeta)
    {
        var nugetDependencies = addonMeta.Addon.GetDependencies()
            .Where(x => x is NuGetDependency)
            .Cast<NuGetDependency>().ToList();

        var addonFolder = Path.Combine(AppConfig.AppDataFolderPath, "Addons", addonMeta.Addon.Identifier);
        if (Directory.Exists(addonFolder)) Directory.CreateDirectory(addonFolder);

        foreach (var dependency in nugetDependencies)
        {
            var dependencyName = dependency.PackageId;
            var nameSpace = dependency.NameSpace;
            var dependencyVersion = dependency.Version == null ? null : new NuGetVersion(dependency.Version);

            var dependencyPath = Path.Combine(addonFolder, $"{nameSpace}.dll");
            if (File.Exists(dependencyPath)) continue;

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

            // it's a nupkg file, we need to extract it
            var tempPath = Path.Combine(addonFolder, $"{dependencyName}_{latestVersion}.nupkg");
            await File.WriteAllBytesAsync(tempPath, packageStream.ToArray());

            // extract the nupkg file
            await using FileStream inputStream = new FileStream(tempPath, FileMode.Open);
            using PackageArchiveReader reader = new PackageArchiveReader(inputStream);
            string? nuspecFile = null;
            string? namespaceName = null;
            foreach (var file in reader.GetFiles())
            {
                if (!file.EndsWith(".dll"))
                    continue;
                nuspecFile = file;
                namespaceName = Path.GetFileNameWithoutExtension(file);
                break;
            }
            if (nuspecFile == null)
            {
                SkEditorAPI.Logs.Error($"Failed to find the nuspec file for {dependencyName}");
                addonMeta.Errors.Add(LoadingErrors.LoadingException(new Exception("Failed to find the nuspec file")));
                return false;
            }

            var filePath = Path.Combine(addonFolder, $"{namespaceName}.dll");
            await using FileStream outputStream = new FileStream(filePath, FileMode.Create);
            await reader.GetStream(nuspecFile).CopyToAsync(outputStream);

            // delete the temp file
            inputStream.Close();
            File.Delete(tempPath);

            SkEditorAPI.Logs.Info($"Dependency {dependencyName} downloaded and stored in the addon folder");
            addonMeta.NeedsRestart = true;
        }

        return true;
    }

}