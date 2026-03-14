using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using SkEditor.Services;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Styling;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Views.Windows.Marketplace.Types;

public class ThemeItem : MarketplaceItem
{
    [JsonIgnore]
    public const string FolderName = "Themes";

    public override string ItemType => "Theme";

    [JsonPropertyName("file")]
    public required string ItemFileUrl { get; set; }

    public override async Task Install()
    {
        string fileName = ItemFileUrl.Split('/').Last();
        string filePath = Path.Combine(SkEditorAPI.Configs.GetAppDataPath(), FolderName, fileName);

        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(ItemFileUrl);
        try
        {
            await using Stream stream = await response.Content.ReadAsStreamAsync();
            await using FileStream fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);

            string message = Translation.Get(TranslationKey.MarketplaceInstallSuccess, ItemName);
            message += "\n" + Translation.Get(TranslationKey.MarketplaceInstallEnableNow);

            ContentDialogResult result = await SkEditorAPI.Windows.ShowDialog("Success", message,
                primaryButtonText: "MarketplaceEnableNow", cancelButtonText: "Okay",
                icon: new SymbolIconSource { Symbol = Symbol.Checkmark });

            if (result == ContentDialogResult.Primary)
            {
                _ = Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    Theme theme = ThemeService.ServiceStatic.LoadTheme(filePath);
                    await ThemeService.ServiceStatic.SetTheme(theme);
                });
            }

            MarketplaceWindow.Instance.HideAllButtons();
            MarketplaceWindow.Instance.ItemView.UninstallButton.IsVisible = true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to install theme!");
            await SkEditorAPI.Windows.ShowError(Translation.Get(TranslationKey.MarketplaceInstallFailed, ItemName));
        }
    }

    public override async Task Uninstall()
    {
        string fileName = ItemFileUrl.Split('/').Last();

        if (fileName.Equals(ThemeService.ServiceStatic.CurrentTheme.FileName))
        {
            await ThemeService.ServiceStatic.SetTheme(
                ThemeService.ServiceStatic.Themes.FirstOrDefault(x => x.FileName.Equals("Default.json")) ??
                ThemeService.GetDefaultTheme());
        }

        Theme? theme = ThemeService.ServiceStatic.Themes.FirstOrDefault(x => x.FileName.Equals(fileName));
        if (theme == null)
        {
            await SkEditorAPI.Windows.ShowError("The theme is not installed.");
            return;
        }

        ThemeService.ServiceStatic.Themes.Remove(theme);
        ThemeService.ServiceStatic.SaveAllThemes();
        File.Delete(Path.Combine(SkEditorAPI.Configs.GetAppDataPath(), "Themes", fileName));

        MarketplaceWindow.Instance.HideAllButtons();
        MarketplaceWindow.Instance.ItemView.InstallButton.IsVisible = true;

        await SkEditorAPI.Windows.ShowDialog(Translation.Get(TranslationKey.Success),
            Translation.Get(TranslationKey.MarketplaceUninstallSuccess, ItemName),
            new SymbolIconSource { Symbol = Symbol.Checkmark });
    }

    public override bool IsInstalled()
    {
        string themePath =
            Path.Combine(SkEditorAPI.Configs.GetAppDataPath(), FolderName, Path.GetFileName(ItemFileUrl));
        return File.Exists(themePath);
    }
}