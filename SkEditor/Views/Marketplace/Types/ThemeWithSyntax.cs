using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Styling;
using SkEditor.Utilities.Syntax;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace SkEditor.Views.Marketplace.Types;
public class ThemeWithSyntaxItem : MarketplaceItem
{
    [JsonProperty("themeFile")]
    public string ThemeFileUrl { get; set; }

    [JsonProperty("syntaxFile")]
    public string SyntaxFileUrl { get; set; }

    public async override void Install()
    {
        string themeFileName = ThemeFileUrl.Split('/').Last();
        string themeFilePath = Path.Combine(AppConfig.AppDataFolderPath, "Themes", themeFileName);
        string syntaxFileName = SyntaxFileUrl.Split('/').Last();
        string syntaxFilePath = Path.Combine(AppConfig.AppDataFolderPath, "Syntax highlighting", syntaxFileName);

        Install(ThemeFileUrl, themeFilePath);
        Install(SyntaxFileUrl, syntaxFilePath);

        string message = Translation.Get("MarketplaceInstallSuccess", ItemName);
        message += "\n" + Translation.Get("MarketplaceInstallEnableNow");

        ContentDialogResult result = await ApiVault.Get().ShowMessageWithIcon("Success", message,
            new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButtonContent: "MarketplaceEnableNow",
            closeButtonContent: "Okay");

        if (result == ContentDialogResult.Primary)
        {
            _ = Dispatcher.UIThread.InvokeAsync(() =>
            {
                Theme theme = ThemeEditor.LoadTheme(themeFilePath);
                ThemeEditor.SetTheme(theme);
            });

            SyntaxLoader.Syntaxes.Add(syntaxFileName);
            ApiVault.Get().GetAppConfig().CurrentSyntax = syntaxFileName;
            _ = Dispatcher.UIThread.InvokeAsync(() => SyntaxLoader.UpdateSyntax(SyntaxLoader.SyntaxFilePath));
        }

        MarketplaceWindow.Instance.HideAllButtons();
        MarketplaceWindow.Instance.ItemView.UninstallButton.IsVisible = true;
    }

    private async void Install(string url, string filePath)
    {
        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(url);
        try
        {
            using Stream stream = await response.Content.ReadAsStreamAsync();
            using FileStream fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);
            await stream.DisposeAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, $"Failed to install {ItemName}!");
            ApiVault.Get().ShowMessage(Translation.Get("Error"), Translation.Get("MarketplaceInstallFailed", ItemName));
        }
    }

    public async override void Uninstall()
    {
        UninstallTheme();
        UninstallSyntax();

        MarketplaceWindow.Instance.HideAllButtons();
        MarketplaceWindow.Instance.ItemView.InstallButton.IsVisible = true;

        await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Success"), Translation.Get("MarketplaceUninstallSuccess", ItemName),
            new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButton: false, closeButtonContent: "Okay");
    }

    private void UninstallTheme()
    {
        string fileName = ThemeFileUrl.Split('/').Last();

        if (fileName.Equals(ThemeEditor.CurrentTheme.FileName))
            ThemeEditor.SetTheme(ThemeEditor.Themes.FirstOrDefault(x => x.FileName.Equals("Default.json")) ?? ThemeEditor.GetDefaultTheme());

        ThemeEditor.Themes.Remove(ThemeEditor.Themes.FirstOrDefault(x => x.FileName.Equals(fileName)));
        ThemeEditor.SaveAllThemes();
        File.Delete(Path.Combine(AppConfig.AppDataFolderPath, "Themes", fileName));
    }

    private async void UninstallSyntax()
    {
        string fileName = SyntaxFileUrl.Split('/').Last();

        SyntaxLoader.Syntaxes.Remove(fileName);
        SyntaxLoader.SetDefaultSyntax();
        await Dispatcher.UIThread.InvokeAsync(() => SyntaxLoader.UpdateSyntax(SyntaxLoader.SyntaxFilePath));
        File.Delete(Path.Combine(AppConfig.AppDataFolderPath, "Syntax Highlighting", fileName));
    }
}