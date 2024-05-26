using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using SkEditor.Utilities.InternalAPI;

namespace SkEditor.Views.Marketplace.Types;

public class ZipAddonItem : AddonItem
{
    [JsonIgnore]
    private const string FolderName = "Addons";

    public async override void Install()
    {
        string fileName = ItemFileUrl.Split('/').Last();
        string filePath = Path.Combine(AppConfig.AppDataFolderPath, FolderName, fileName);

        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(ItemFileUrl);
        try
        {
            using Stream stream = await response.Content.ReadAsStreamAsync();
            using FileStream fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);
            await stream.DisposeAsync();
            await fileStream.DisposeAsync();

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

            await ApiVault.Get().ShowMessageWithIcon("Success", message,
                new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButton: false, closeButtonContent: "Okay");

            MarketplaceWindow.Instance.HideAllButtons();
            MarketplaceWindow.Instance.ItemView.UninstallButton.IsVisible = true;
            MarketplaceWindow.Instance.ItemView.DisableButton.IsVisible = true;
            RunAddon();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to install addon!");
            ApiVault.Get().ShowMessage(Translation.Get("Error"), Translation.Get("MarketplaceInstallFailed", ItemName));
        }
    }

    private void RunAddon()
    {
        if (ItemRequiresRestart) return;

        string fileName = ItemFileUrl.Split('/').Last();
        string addonDirectory = Path.Combine(AppConfig.AppDataFolderPath, FolderName, Path.GetFileNameWithoutExtension(fileName));
        if (!Directory.Exists(addonDirectory))
        {
            Log.Error($"Addon directory '{addonDirectory}' does not exist!"); return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            var packagesFolder = Path.Combine(addonDirectory, "Packages");
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

    public async override void Uninstall()
    {
        string fileName = Path.GetFileNameWithoutExtension(ItemFileUrl.Split('/').Last());

        ApiVault.Get().GetAppConfig().AddonsToDelete.Add(Path.GetFileNameWithoutExtension(fileName));
        ApiVault.Get().GetAppConfig().Save();

        MarketplaceWindow.Instance.ItemView.UninstallButton.IsEnabled = false;

        await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Success"), Translation.Get("MarketplaceUninstallSuccess",
            ItemName), new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButton: false, closeButtonContent: "Okay");
    }

    public new async void Update()
    {
        string fileName = "updated-" + ItemFileUrl.Split('/').Last();
        ApiVault.Get().GetAppConfig().AddonsToUpdate.Add(fileName);
        ApiVault.Get().GetAppConfig().Save();
        MarketplaceWindow.Instance.ItemView.UpdateButton.IsEnabled = false;

        string filePath = Path.Combine(AppConfig.AppDataFolderPath, "Addons", fileName);

        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(ItemFileUrl);

        try
        {
            using Stream stream = await response.Content.ReadAsStreamAsync();
            using FileStream fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);

            ContentDialogResult result = await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Success"), Translation.Get("MarketplaceUpdateSuccess", ItemName),
                new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButton: false, closeButtonContent: "Okay");
        }
        catch
        {
            ApiVault.Get().ShowMessage(Translation.Get("Error"), Translation.Get("MarketplaceUpdateFailed", ItemName));
        }
    }

    public new void Disable()
    {
        string fileName = ItemFileUrl.Split('/').Last().Replace(".zip", "");
        ApiVault.Get().GetAppConfig().AddonsToDisable.Add(fileName);
        ApiVault.Get().GetAppConfig().Save();
        MarketplaceWindow.Instance.ItemView.DisableButton.IsVisible = false;
        MarketplaceWindow.Instance.ItemView.EnableButton.IsVisible = true;
    }

    public new void Enable()
    {
        string fileName = ItemFileUrl.Split('/').Last().Replace(".zip", "");
        ApiVault.Get().GetAppConfig().AddonsToDisable.Remove(fileName);
        ApiVault.Get().GetAppConfig().Save();
        MarketplaceWindow.Instance.ItemView.DisableButton.IsVisible = true;
        MarketplaceWindow.Instance.ItemView.EnableButton.IsVisible = false;

        RunAddon();
    }
}