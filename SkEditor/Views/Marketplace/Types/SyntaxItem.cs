using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SkEditor.Views.Marketplace.Types;
public class SyntaxItem : MarketplaceItem
{
    [JsonProperty("syntaxFolders")]
    public string[] ItemSyntaxFolders { get; set; }

    [JsonIgnore]
    public const string FolderName = "Syntax Highlighting";

    public async override void Install()
    {
        //string baseLocalSyntaxPath = Path.Combine(AppConfig.AppDataFolderPath, FolderName);

        //List<FileSyntax> installedSyntaxes = [];
        //bool allInstalled = true;
        //foreach (string folder in ItemSyntaxFolders)
        //{
        //    var folderName = folder.Split('/').Last();
        //    string localSyntaxPath = Path.Combine(baseLocalSyntaxPath,
        //        folderName);
        //    Directory.CreateDirectory(localSyntaxPath);
        //    allInstalled = allInstalled && await Install(folder + "/config.json", Path.Combine(localSyntaxPath, "config.json"));
        //    allInstalled = allInstalled && await Install(folder + "/syntax.xshd", Path.Combine(localSyntaxPath, "syntax.xshd"));

        //    if (!allInstalled)
        //        break;

        //    try
        //    {
        //        ApiVault.Get().Log("Load syntax called ");
        //        installedSyntaxes.Add(await SyntaxLoader.LoadSyntax(localSyntaxPath));
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error(e, $"Failed to load syntax {folderName}!");
        //        ApiVault.Get().ShowMessage(Translation.Get("Error"), Translation.Get("MarketplaceInstallFailed", ItemName));
        //        return;
        //    }
        //}

        //if (!allInstalled)
        //{
        //    ApiVault.Get().ShowMessage(Translation.Get("Error"), Translation.Get("MarketplaceInstallFailed", ItemName));
        //    return;
        //}

        //string message = Translation.Get("MarketplaceInstallSuccess", ItemName);
        //message += "\n" + Translation.Get("MarketplaceInstallEnableNow");

        //ContentDialogResult result = await ApiVault.Get().ShowMessageWithIcon("Success", message,
        //    new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButtonContent: "MarketplaceEnableNow",
        //    closeButtonContent: "Okay");

        //if (result == ContentDialogResult.Primary)
        //{
        //    foreach (var syntax in installedSyntaxes)
        //    {
        //        SyntaxLoader.SelectSyntax(syntax);
        //    }

        //    SyntaxLoader.RefreshAllOpenedEditors();
        //}

        //MarketplaceWindow.Instance.HideAllButtons();
        //MarketplaceWindow.Instance.ItemView.UninstallButton.IsVisible = true;
    }

    private async Task<bool> Install(string url, string filePath)
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
            return false;
        }

        return true;
    }

    public override async void Uninstall()
    {
        //List<string> folders = ItemSyntaxFolders.ToList();
        //var syntaxFolder = Path.Combine(AppConfig.AppDataFolderPath, FolderName);
        //foreach (string folder in folders)
        //{
        //    var folderName = folder.Split('/').Last();
        //    string localSyntaxPath = Path.Combine(syntaxFolder,
        //        folderName);

        //    await SyntaxLoader.UnloadSyntax(localSyntaxPath);
        //    Directory.Delete(localSyntaxPath, true);
        //}
        //SyntaxLoader.CheckConfiguredFileSyntaxes();
        //SyntaxLoader.RefreshAllOpenedEditors();

        //MarketplaceWindow.Instance.HideAllButtons();
        //MarketplaceWindow.Instance.ItemView.InstallButton.IsVisible = true;

        //await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Success"), Translation.Get("MarketplaceUninstallSuccess", ItemName),
        //    new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButton: false, closeButtonContent: "Okay");
    }

    public override bool IsInstalled()
    {
        var syntaxFolder = Path.Combine(AppConfig.AppDataFolderPath, FolderName);
        foreach (string folder in ItemSyntaxFolders)
        {
            var folderName = folder.Split('/').Last();
            string localSyntaxPath = Path.Combine(syntaxFolder,
                folderName);
            if (!Directory.Exists(localSyntaxPath))
                return false;
        }

        return true;
    }
}