using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Threading;
using Newtonsoft.Json;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Views.Windows.Marketplace.Types;

public class ZipAddonItem : AddonItem
{
    [JsonIgnore] private const string FolderName = "Addons";

    public override async Task Install()
    {
        string fileName = ItemFileUrl.Split('/').Last();
        string filePath = Path.Combine(AppConfig.AppDataFolderPath, FolderName, fileName);

        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(ItemFileUrl);
        try
        {
            await using Stream stream = await response.Content.ReadAsStreamAsync();
            await using FileStream fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);

            ZipFile.ExtractToDirectory(filePath, Path.Combine(AppConfig.AppDataFolderPath, FolderName));
            File.Delete(filePath);

            string message = Translation.Get("MarketplaceInstallSuccess", ItemName);

            if (ItemRequiresRestart)
            {
                message += "\n" + Translation.Get("MarketplaceInstallRestart");
            }
            else
            {
                message += "\n" + Translation.Get("MarketplaceInstallNoNeedToRestart");
            }

            await SkEditorAPI.Windows.ShowDialog(Translation.Get("Success"), message,
                new SymbolIconSource { Symbol = Symbol.CheckmarkCircle }, primaryButtonText: "Okay");

            MarketplaceWindow.Instance.HideAllButtons();
            MarketplaceWindow.Instance.ItemView.UninstallButton.IsVisible = true;
            MarketplaceWindow.Instance.ItemView.DisableButton.IsVisible = true;
            RunAddon();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to install addon!");
            await SkEditorAPI.Windows.ShowMessage(Translation.Get("Error"),
                Translation.Get("MarketplaceInstallFailed", ItemName));
        }
    }

    private void RunAddon()
    {
        if (ItemRequiresRestart)
        {
            return;
        }

        string fileName = ItemFileUrl.Split('/').Last();
        string addonDirectory = Path.Combine(AppConfig.AppDataFolderPath, FolderName,
            Path.GetFileNameWithoutExtension(fileName));
        if (!Directory.Exists(addonDirectory))
        {
            Log.Error("Addon directory '{AddonDirectory}' does not exist!", addonDirectory);
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            string packagesFolder = Path.Combine(addonDirectory, "Packages");
            if (Directory.Exists(packagesFolder))
            {
                //AddonLoader.LoadAddonsFromFolder(packagesFolder);
            }
            /*List<Assembly> assemblies = AddonLoader.LoadAddonsFromFolder(addonDirectory);

            assemblies.ForEach(assembly =>
            {
                if (assembly.GetTypes().FirstOrDefault(p => typeof(IAddon).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract) is Type addonType)
                {
                    IAddon addon = (IAddon)Activator.CreateInstance(addonType);
                    AddonLoader.EnabledAddons.Add(addon);
                    addon.OnEnable();
                }
                else
                {
                    Log.Error($"Failed to enable addon '{ItemName}'!");
                }
            });*/
        });
    }

    public override async Task Uninstall()
    {
        MarketplaceWindow.Instance.ItemView.UninstallButton.IsEnabled = false;

        await SkEditorAPI.Windows.ShowDialog(Translation.Get("Success"),
            Translation.Get("MarketplaceUninstallSuccess", ItemName),
            new SymbolIconSource { Symbol = Symbol.CheckmarkCircle }, primaryButtonText: "Okay");
    }

    public async Task Update()
    {
        string fileName = "updated-" + ItemFileUrl.Split('/').Last();
        SkEditorAPI.Core.GetAppConfig().Save();
        MarketplaceWindow.Instance.ItemView.UpdateButton.IsEnabled = false;

        string filePath = Path.Combine(AppConfig.AppDataFolderPath, "Addons", fileName);

        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(ItemFileUrl);

        try
        {
            await using Stream stream = await response.Content.ReadAsStreamAsync();
            await using FileStream fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);

            await SkEditorAPI.Windows.ShowDialog(Translation.Get("Success"),
                Translation.Get("MarketplaceUpdateSuccess", ItemName),
                new SymbolIconSource { Symbol = Symbol.CheckmarkCircle }, primaryButtonText: "Okay");
        }
        catch
        {
            await SkEditorAPI.Windows.ShowMessage(Translation.Get("Error"),
                Translation.Get("MarketplaceUpdateFailed", ItemName));
        }
    }

    public void Disable()
    {
        SkEditorAPI.Core.GetAppConfig().Save();
        MarketplaceWindow.Instance.ItemView.DisableButton.IsVisible = false;
        MarketplaceWindow.Instance.ItemView.EnableButton.IsVisible = true;
    }

    public void Enable()
    {
        SkEditorAPI.Core.GetAppConfig().Save();
        MarketplaceWindow.Instance.ItemView.DisableButton.IsVisible = true;
        MarketplaceWindow.Instance.ItemView.EnableButton.IsVisible = false;

        RunAddon();
    }
}