﻿using FluentAvalonia.UI.Controls;
using Newtonsoft.Json;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Views.Settings;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace SkEditor.Views.Marketplace.Types;

public class AddonItem : MarketplaceItem
{
    [JsonProperty("requiresRestart")] public bool ItemRequiresRestart { get; set; }
    [JsonProperty("file")] public string ItemFileUrl { get; set; }

    [JsonIgnore] private const string FolderName = "Addons";

    public override async void Install()
    {
        var fileName = ItemFileUrl.Split('/').Last();
        var addonIdentifier = Path.GetFileNameWithoutExtension(fileName);

        Directory.CreateDirectory(Path.Combine(AppConfig.AppDataFolderPath, FolderName, addonIdentifier));
        var filePath = Path.Combine(AppConfig.AppDataFolderPath, FolderName, addonIdentifier, addonIdentifier + ".dll");

        try
        {
            using HttpClient client = new();
            var response = await client.GetAsync(ItemFileUrl);
            await using var stream = await response.Content.ReadAsStreamAsync();
            await using var fileStream = File.Create(filePath);
            await stream.CopyToAsync(fileStream);
            fileStream.Close();
            stream.Close();

            var message = Translation.Get("MarketplaceInstallSuccess", ItemName);

            if (ItemRequiresRestart)
            {
                message += "\n" + Translation.Get("MarketplaceInstallRestart");
            }
            else
            {
                message += "\n" + Translation.Get("MarketplaceInstallNoNeedToRestart");
            }

            await SkEditorAPI.Windows.ShowDialog(Translation.Get("Success"), message,
                new SymbolIconSource() { Symbol = Symbol.Accept }, primaryButtonText: "Okay");

            RunAddon(addonIdentifier);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to install addon!");
            await SkEditorAPI.Windows.ShowMessage(Translation.Get("Error"), Translation.Get("MarketplaceInstallFailed", ItemName));
        }

        Marketplace.RefreshCurrentSelection();
    }

    private async void RunAddon(string addonIdentifier)
    {
        if (ItemRequiresRestart)
            return;
        var folder = Path.Combine(AppConfig.AppDataFolderPath, FolderName, addonIdentifier);

        await AddonLoader.LoadAddonFromFile(folder);
    }

    public override void Uninstall()
    {
        throw new NotImplementedException();
    }

    public void Manage()
    {
        SkEditorAPI.Windows.ShowWindow(new SettingsWindow());
        SettingsWindow.NavigateToPage(typeof(AddonsPage));
    }

    public override bool IsInstalled()
    {
        var fileName = ItemFileUrl.Split('/').Last();
        var addonIdentifier = Path.GetFileNameWithoutExtension(fileName);

        return SkEditorAPI.Addons.GetAddon(addonIdentifier) != null;
    }
}