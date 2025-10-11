using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Views.Windows.Settings;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Views.Windows.Marketplace.Types;

public class AddonItem : MarketplaceItem
{
    [JsonIgnore] private const string FolderName = "Addons";
    [JsonProperty("requiresRestart")] public bool ItemRequiresRestart { get; set; }
    [JsonProperty("file")] public required string ItemFileUrl { get; set; }

    public override async Task Install()
    {
        string fileName = ItemFileUrl.Split('/').Last();
        string addonIdentifier = Path.GetFileNameWithoutExtension(fileName);

        Directory.CreateDirectory(Path.Combine(AppConfig.AppDataFolderPath, FolderName, addonIdentifier));
        string filePath = Path.Combine(AppConfig.AppDataFolderPath, FolderName, addonIdentifier,
            addonIdentifier + ".dll");

        try
        {
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(ItemFileUrl);
            await using Stream stream = await response.Content.ReadAsStreamAsync();
            await using FileStream fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);
            fileStream.Close();
            stream.Close();

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

            await RunAddon(addonIdentifier);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to install addon!");
            await SkEditorAPI.Windows.ShowMessage(Translation.Get("Error"),
                Translation.Get("MarketplaceInstallFailed", ItemName));
        }

        Marketplace.RefreshCurrentSelection();
    }

    private async Task RunAddon(string addonIdentifier)
    {
        if (ItemRequiresRestart)
        {
            return;
        }

        string folder = Path.Combine(AppConfig.AppDataFolderPath, FolderName, addonIdentifier);

        await AddonLoader.LoadAddonFromFile(folder);
    }

    public override Task Uninstall()
    {
        throw new NotImplementedException();
    }

    public static void Manage()
    {
        SkEditorAPI.Windows.ShowWindow(new Settings.SettingsWindow());
        Settings.SettingsWindow.NavigateToPage(typeof(AddonsPage));
    }

    public override bool IsInstalled()
    {
        return GetAddon() != null;
    }
    
    public IAddon? GetAddon()
    {
        string fileName = ItemFileUrl.Split('/').Last();
        string addonIdentifier = Path.GetFileNameWithoutExtension(fileName);

        return SkEditorAPI.Addons.GetAddon(addonIdentifier);
    }

    public async Task Update()
    {
        IAddon? addon = GetAddon();
        if (addon == null)
        {
            await Install();
            return;
        }

        await AddonLoader.DeleteAddon(addon);
        await Install();
    }
}