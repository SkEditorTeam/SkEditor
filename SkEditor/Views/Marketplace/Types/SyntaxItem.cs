using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Syntax;

namespace SkEditor.Views.Marketplace.Types;

public class SyntaxItem : MarketplaceItem
{
    [JsonIgnore] public const string FolderName = "Syntax Highlighting";

    [JsonProperty("syntaxFolders")] public string[] ItemSyntaxFolders { get; set; }

    public override async Task Install()
    {
        string baseLocalSyntaxPath = Path.Combine(AppConfig.AppDataFolderPath, FolderName);

        List<FileSyntax> installedSyntaxes = [];
        bool allInstalled = true;
        foreach (string folder in ItemSyntaxFolders)
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
            await SkEditorAPI.Windows.ShowError(Translation.Get("MarketplaceInstallFailed", ItemName));
            return;
        }

        string message = Translation.Get("MarketplaceInstallSuccess", ItemName);
        message += "\n" + Translation.Get("MarketplaceInstallEnableNow");

        ContentDialogResult result = await SkEditorAPI.Windows.ShowDialog("Success", message,
            primaryButtonText: "MarketplaceEnableNow", cancelButtonText: "Okay");

        if (result == ContentDialogResult.Primary)
        {
            foreach (FileSyntax syntax in installedSyntaxes)
            {
                await SyntaxLoader.SelectSyntax(syntax);
            }

            SyntaxLoader.RefreshAllOpenedEditors();
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
            Log.Error(e, $"Failed to install {ItemName}!");
            await SkEditorAPI.Windows.ShowError(Translation.Get("MarketplaceInstallFailed", ItemName));
            return false;
        }

        return true;
    }

    public override async Task Uninstall()
    {
        List<string> folders = ItemSyntaxFolders.ToList();
        string syntaxFolder = Path.Combine(AppConfig.AppDataFolderPath, FolderName);
        foreach (string localSyntaxPath in folders
                     .Select(folder => folder.Split('/').Last())
                     .Select(folderName => Path.Combine(syntaxFolder,
                         folderName)))
        {
            await SyntaxLoader.UnloadSyntax(localSyntaxPath);
            Directory.Delete(localSyntaxPath, true);
        }

        SyntaxLoader.CheckConfiguredFileSyntaxes();
        SyntaxLoader.RefreshAllOpenedEditors();

        MarketplaceWindow.Instance.HideAllButtons();
        MarketplaceWindow.Instance.ItemView.InstallButton.IsVisible = true;

        await SkEditorAPI.Windows.ShowDialog("Success", Translation.Get("MarketplaceUninstallSuccess", ItemName),
            primaryButtonText: "Okay");
    }

    public override bool IsInstalled()
    {
        string syntaxFolder = Path.Combine(AppConfig.AppDataFolderPath, FolderName);
        return ItemSyntaxFolders
            .Select(folder => folder.Split('/').Last())
            .Select(folderName => Path.Combine(syntaxFolder, folderName))
            .All(Directory.Exists);
    }
}