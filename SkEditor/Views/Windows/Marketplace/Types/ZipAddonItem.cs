using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.InternalAPI;
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
        
        try
        {
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(ItemFileUrl);
            await using Stream stream = await response.Content.ReadAsStreamAsync();
            await using FileStream fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);
            fileStream.Close();
            stream.Close();

            var addonFolderName = Path.GetFileNameWithoutExtension(filePath);
            ZipFile.ExtractToDirectory(filePath, Path.Combine(AppConfig.AppDataFolderPath, FolderName, addonFolderName));
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
                new SymbolIconSource { Symbol = Symbol.Checkmark }, primaryButtonText: "Okay");

            await EnableAddon();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to install addon!");
            await SkEditorAPI.Windows.ShowMessage(Translation.Get("Error"),
                Translation.Get("MarketplaceInstallFailed", ItemName));
        }
        
        Marketplace.RefreshCurrentSelection();
    }

    private async Task EnableAddon()
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

        await AddonLoader.LoadAddonFromFile(addonDirectory);
    }

    public override Task Uninstall()
    {
        throw new NotImplementedException();
    }

    public override bool IsInstalled()
    {
        return GetAddon() != null;
    }
    
    public new IAddon? GetAddon()
    {
        string fileName = ItemFileUrl.Split('/').Last();
        string addonIdentifier = Path.GetFileNameWithoutExtension(fileName);

        return SkEditorAPI.Addons.GetAddon(addonIdentifier);
    }

    public override async Task Update()
    {
        var addon = GetAddon();

        if (addon == null)
        {
            return;
        }

        await AddonLoader.DeleteAddon(addon);

        var addonFolderPath = Path.Combine(AppConfig.AppDataFolderPath, FolderName, addon.Identifier);

        Directory.Delete(addonFolderPath, true);

        await Install();

        SkEditorAPI.Core.GetAppConfig().Save();
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

        _ = EnableAddon();
    }
}