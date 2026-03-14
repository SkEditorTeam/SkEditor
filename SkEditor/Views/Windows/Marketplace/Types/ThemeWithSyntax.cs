using System;
using System.Collections.Generic;
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
using SkEditor.Utilities.Syntax;

namespace SkEditor.Views.Windows.Marketplace.Types;

public class ThemeWithSyntaxItem : MarketplaceItem
{
    public override string ItemType => "ThemeWithSyntax";

    [JsonPropertyName("themeFile")]
    public required string ThemeFileUrl { get; set; }

    [JsonPropertyName("syntaxFolders")]
    public required string[] SyntaxFolders { get; set; }

    public override async Task Install()
    {
        string themeFileName = ThemeFileUrl.Split('/').Last();
        string themeFilePath = Path.Combine(SkEditorAPI.Configs.GetAppDataPath(), "Themes", themeFileName);
        string baseLocalSyntaxPath = Path.Combine(SkEditorAPI.Configs.GetAppDataPath(), "Syntax Highlighting");
        //string syntaxFilePath = Path.Combine(AppConfig.AppDataFolderPath, "Syntax highlighting", syntaxFileName);

        List<FileSyntax> installedSyntaxes = [];
        bool allInstalled = true;
        allInstalled = allInstalled && await Install(ThemeFileUrl, themeFilePath);
        foreach (string folder in SyntaxFolders)
        {
            string folderName = folder.Split('/').Last();
            string localSyntaxPath = Path.Combine(baseLocalSyntaxPath,
                folderName);
            Directory.CreateDirectory(localSyntaxPath);
            allInstalled = allInstalled &&
                           await Install(folder + "/config.json", Path.Combine(localSyntaxPath, "config.json"));
            allInstalled = allInstalled &&
                           await Install(folder + "/syntax.xshd", Path.Combine(localSyntaxPath, "syntax.xshd"));

            if (!allInstalled)
            {
                break;
            }

            try
            {
                installedSyntaxes.Add(await SyntaxLoader.LoadSyntax(localSyntaxPath));
            }
            catch (Exception)
            {
                SkEditorAPI.Logs.Error($"Failed to load syntax {folderName}!", true);
                return;
            }
        }

        if (!allInstalled)
        {
            await SkEditorAPI.Windows.ShowError(Translation.Get(TranslationKey.MarketplaceInstallFailed, ItemName));
            return;
        }

        string message = Translation.Get(TranslationKey.MarketplaceInstallSuccess, ItemName);
        message += "\n" + Translation.Get(TranslationKey.MarketplaceInstallEnableNow);

        ContentDialogResult result = await SkEditorAPI.Windows.ShowDialog("Success", message,
            primaryButtonText: "MarketplaceEnableNow", cancelButtonText: "Okay");

        if (result == ContentDialogResult.Primary)
        {
            _ = Dispatcher.UIThread.InvokeAsync(async () =>
            {
                Theme theme = ThemeService.ServiceStatic.LoadTheme(themeFilePath);
                await ThemeService.ServiceStatic.SetTheme(theme);
            });

            foreach (FileSyntax syntax in installedSyntaxes)
            {
                await SyntaxLoader.SelectSyntax(syntax);
            }
        }

        MarketplaceWindow.Instance.HideAllButtons();
        MarketplaceWindow.Instance.ItemView.UninstallButton.IsVisible = true;
    }

    private async Task<bool> Install(string url, string filePath)
    {
        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(url);
        try
        {
            await using Stream stream = await response.Content.ReadAsStreamAsync();
            await using FileStream fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to install {S}!", ItemName);
            await SkEditorAPI.Windows.ShowError(Translation.Get(TranslationKey.MarketplaceInstallFailed, ItemName));
            return false;
        }

        return true;
    }

    public override async Task Uninstall()
    {
        await UninstallTheme();
        await UninstallSyntax();

        MarketplaceWindow.Instance.HideAllButtons();
        MarketplaceWindow.Instance.ItemView.InstallButton.IsVisible = true;

        await SkEditorAPI.Windows.ShowDialog("Success",
            Translation.Get(TranslationKey.MarketplaceUninstallSuccess, ItemName),
            primaryButtonText: "Okay");
    }

    private async Task UninstallTheme()
    {
        string fileName = ThemeFileUrl.Split('/').Last();

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
    }

    private async Task UninstallSyntax()
    {
        List<string> folders = SyntaxFolders.ToList();
        string syntaxFolder = Path.Combine(SkEditorAPI.Configs.GetAppDataPath(), "Syntax Highlighting");
        foreach (string localSyntaxPath in folders
                     .Select(folder => folder.Split('/').Last())
                     .Select(folderName => Path.Combine(syntaxFolder, folderName)))
        {
            await SyntaxLoader.UnloadSyntax(localSyntaxPath);
            Directory.Delete(localSyntaxPath, true);
        }

        SyntaxLoader.CheckConfiguredFileSyntaxes();
        SyntaxLoader.RefreshAllOpenedEditors();
    }

    public override bool IsInstalled()
    {
        string syntaxFolder = Path.Combine(SkEditorAPI.Configs.GetAppDataPath(), "Syntax Highlighting");
        foreach (string folder in SyntaxFolders)
        {
            string folderName = folder.Split('/').Last();
            string localSyntaxPath = Path.Combine(syntaxFolder,
                folderName);
            if (!Directory.Exists(localSyntaxPath))
            {
                return false;
            }
        }

        // Also check if theme is installed
        string themePath = Path.Combine(SkEditorAPI.Configs.GetAppDataPath(), ThemeItem.FolderName,
            Path.GetFileName(ThemeFileUrl));
        if (!File.Exists(themePath))
        {
            return false;
        }

        return true;
    }
}