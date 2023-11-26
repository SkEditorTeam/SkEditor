using Serilog;
using SkEditor.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SkEditor.API;
public class AddonLoader
{
	public static List<IAddon> Addons { get; } = [];

	public static void Load()
	{
		var addonFolder = Path.Combine(AppConfig.AppDataFolderPath, "Addons");

		Directory.CreateDirectory(addonFolder);

		IEnumerable<string> addonFolders = Directory.EnumerateDirectories(addonFolder, "*", SearchOption.TopDirectoryOnly);

		addonFolders.ToList().ForEach(folder =>
		{
			var packagesFolder = Path.Combine(folder, "Packages");
			if (Directory.Exists(packagesFolder))
			{
				LoadAddonsFromFolder(packagesFolder);
			}
			LoadAddonsFromFolder(folder);
		});

		LoadAddonsFromFolder(addonFolder);

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
		}
		catch (Exception ex)
		{
			Log.Error(ex, "Failed to load addons");
		}
	}

	private static void LoadAddonsFromFolder(string folder)
	{
		IEnumerable<string> dllFiles = Directory.EnumerateFiles(folder, "*.dll", SearchOption.TopDirectoryOnly);

		foreach (string updatedFile in dllFiles.Where(f => Path.GetFileName(f).StartsWith("updated-")))
		{
			string nameWithoutPrefix = Path.GetFileName(updatedFile)["updated-".Length..];
			string fileWithoutPrefixPath = Path.Combine(folder, nameWithoutPrefix);
			File.Delete(fileWithoutPrefixPath);
			File.Move(updatedFile, fileWithoutPrefixPath);
		}

		dllFiles.ToList().ForEach(dllFile =>
		{
			string fileName = Path.GetFileName(dllFile);

			if (ApiVault.Get().GetAppConfig().AddonsToDelete.Contains(fileName))
			{
				File.Delete(dllFile);
				ApiVault.Get().GetAppConfig().AddonsToDisable.Remove(fileName);
				ApiVault.Get().GetAppConfig().AddonsToDelete.Remove(fileName);
				return;
			}
			else if (ApiVault.Get().GetAppConfig().AddonsToDisable.Contains(fileName)) return;
			Assembly.LoadFrom(dllFile);
		});
	}
}
