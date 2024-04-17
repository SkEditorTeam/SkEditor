using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace SkEditor.Views.Marketplace.Types;

public class AddonItem : MarketplaceItem
{
    [JsonProperty("requiresRestart")]
    public bool ItemRequiresRestart { get; set; }

    [JsonProperty("file")]
    public string ItemFileUrl { get; set; }

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
        string filePath = Path.Combine(AppConfig.AppDataFolderPath, FolderName, fileName);

        Dispatcher.UIThread.Post(() =>
        {

            Assembly assembly = Assembly.LoadFrom(filePath);
            IAddon addon = assembly.GetTypes().FirstOrDefault(p => typeof(IAddon).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract) is Type addonType
                ? (IAddon)Activator.CreateInstance(addonType)
                : null;

            if (addon != null)
            {
                AddonLoader.Addons.Add(addon);
                addon.OnEnable();
            }
            else
            {
                Log.Error($"Failed to enable addon '{ItemName}'!");
            }
        });
    }

    public async override void Uninstall()
    {
        string fileName = ItemFileUrl.Split('/').Last();

        ApiVault.Get().GetAppConfig().AddonsToDelete.Add(fileName);
        ApiVault.Get().GetAppConfig().Save();

        MarketplaceWindow.Instance.ItemView.UninstallButton.IsEnabled = false;

        await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Success"), Translation.Get("MarketplaceUninstallSuccess",
            ItemName), new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButton: false, closeButtonContent: "Okay");
    }

    public void Disable()
    {
        string fileName = ItemFileUrl.Split('/').Last();
        ApiVault.Get().GetAppConfig().AddonsToDisable.Add(fileName);
        ApiVault.Get().GetAppConfig().Save();
        MarketplaceWindow.Instance.ItemView.DisableButton.IsVisible = false;
        MarketplaceWindow.Instance.ItemView.EnableButton.IsVisible = true;
    }

    public void Enable()
    {
        string fileName = ItemFileUrl.Split('/').Last();
        ApiVault.Get().GetAppConfig().AddonsToDisable.Remove(fileName);
        ApiVault.Get().GetAppConfig().Save();
        MarketplaceWindow.Instance.ItemView.DisableButton.IsVisible = true;
        MarketplaceWindow.Instance.ItemView.EnableButton.IsVisible = false;
    }

    public async void Update()
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
}