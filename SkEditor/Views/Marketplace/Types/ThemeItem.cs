using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Styling;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace SkEditor.Views.Marketplace.Types;

public class ThemeItem : MarketplaceItem
{
    [JsonProperty("file")]
    public string ItemFileUrl { get; set; }

    [JsonIgnore]
    public const string FolderName = "Themes";

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
            message += "\n" + Translation.Get("MarketplaceInstallEnableNow");

            ContentDialogResult result = await ApiVault.Get().ShowMessageWithIcon("Success", message,
                new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButtonContent: "MarketplaceEnableNow",
                closeButtonContent: "Okay");

            if (result == ContentDialogResult.Primary)
            {
                _ = Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Theme theme = ThemeEditor.LoadTheme(filePath);
                    ThemeEditor.SetTheme(theme);
                });
            }

            MarketplaceWindow.Instance.HideAllButtons();
            MarketplaceWindow.Instance.ItemView.UninstallButton.IsVisible = true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to install theme!");
            ApiVault.Get().ShowMessage(Translation.Get("Error"), Translation.Get("MarketplaceInstallFailed", ItemName));
        }
    }

    public async override void Uninstall()
    {
        string fileName = ItemFileUrl.Split('/').Last();

        if (fileName.Equals(ThemeEditor.CurrentTheme.FileName))
            ThemeEditor.SetTheme(ThemeEditor.Themes.FirstOrDefault(x => x.FileName.Equals("Default.json")) ?? ThemeEditor.GetDefaultTheme());

        ThemeEditor.Themes.Remove(ThemeEditor.Themes.FirstOrDefault(x => x.FileName.Equals(fileName)));
        ThemeEditor.SaveAllThemes();
        File.Delete(Path.Combine(AppConfig.AppDataFolderPath, "Themes", fileName));

        MarketplaceWindow.Instance.HideAllButtons();
        MarketplaceWindow.Instance.ItemView.InstallButton.IsVisible = true;

        await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Success"), Translation.Get("MarketplaceUninstallSuccess", ItemName), new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButton: false, closeButtonContent: "Okay");
    }
}