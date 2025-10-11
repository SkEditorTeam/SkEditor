using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Styling;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Views.Windows.Marketplace.Types;

public class ThemeItem : MarketplaceItem
{
    [JsonIgnore] public const string FolderName = "Themes";

    [JsonProperty("file")] public required string ItemFileUrl { get; set; }

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

            string message = Translation.Get("MarketplaceInstallSuccess", ItemName);
            message += "\n" + Translation.Get("MarketplaceInstallEnableNow");

            ContentDialogResult result = await SkEditorAPI.Windows.ShowDialog("Success", message,
                primaryButtonText: "MarketplaceEnableNow", cancelButtonText: "Okay",
                icon: new SymbolIconSource { Symbol = Symbol.Checkmark });

            if (result == ContentDialogResult.Primary)
            {
                _ = Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    Theme theme = ThemeEditor.LoadTheme(filePath);
                    await ThemeEditor.SetTheme(theme);
                });
            }

            MarketplaceWindow.Instance.HideAllButtons();
            MarketplaceWindow.Instance.ItemView.UninstallButton.IsVisible = true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to install theme!");
            await SkEditorAPI.Windows.ShowError(Translation.Get("MarketplaceInstallFailed", ItemName));
        }
    }

    public override async Task Uninstall()
    {
        string fileName = ItemFileUrl.Split('/').Last();

        if (fileName.Equals(ThemeEditor.CurrentTheme.FileName))
        {
            await ThemeEditor.SetTheme(ThemeEditor.Themes.FirstOrDefault(x => x.FileName.Equals("Default.json")) ??
                                       ThemeEditor.GetDefaultTheme());
        }

        Theme? theme = ThemeEditor.Themes.FirstOrDefault(x => x.FileName.Equals(fileName));
        if (theme == null)
        {
            await SkEditorAPI.Windows.ShowError("The theme is not installed.");
            return;
        }

        ThemeEditor.Themes.Remove(theme);
        ThemeEditor.SaveAllThemes();
        File.Delete(Path.Combine(AppConfig.AppDataFolderPath, "Themes", fileName));

        MarketplaceWindow.Instance.HideAllButtons();
        MarketplaceWindow.Instance.ItemView.InstallButton.IsVisible = true;

        await SkEditorAPI.Windows.ShowDialog(Translation.Get("Success"),
            Translation.Get("MarketplaceUninstallSuccess", ItemName),
            new SymbolIconSource { Symbol = Symbol.Checkmark });
    }

    public override bool IsInstalled()
    {
        string themePath = Path.Combine(AppConfig.AppDataFolderPath, FolderName, Path.GetFileName(ItemFileUrl));
        return File.Exists(themePath);
    }
}