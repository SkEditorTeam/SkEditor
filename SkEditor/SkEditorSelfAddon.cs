using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.ViewModels;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.Avalonia.Fluent.SymbolIcon;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor;

public class SkEditorSelfAddon : IAddon
{
    public string Name => "SkEditorCore";
    public string Version => SettingsViewModel.Version;
    public string Description => "The core of SkEditor, providing the base functionalities.";

    private ImageIconSource _iconSource = null!;
    public IconSource GetAddonIcon()
    {
        if (_iconSource is not null)
            return _iconSource;
        
        Stream stream = AssetLoader.Open(new Uri("avares://SkEditor/Assets/SkEditor.svg"));
        return _iconSource = new ImageIconSource() { Source = new SvgImage { Source = SvgSource.LoadFromStream(stream) } };
    }

    public void OnEnable()
    {
        #region Registries - Connections

        IconSource GetIcon(string fileName, bool svg)
        {
            Stream stream = AssetLoader.Open(new Uri("avares://SkEditor/Assets/Brands/" + fileName));
            return new ImageIconSource()
            {
                Source = svg
                    ? new SvgImage { Source = SvgSource.LoadFromStream(stream) }
                    : new Bitmap(stream)
            };
        }

        Registries.Connections.Register(new RegistryKey(this, "skUnityConnection"),
            new ConnectionData("skUnity", 
                "Used as a documentation provider and a script host (via skUnity Parser)", 
                "SkUnityAPIKey",
                GetIcon("skUnity.svg", true),
                "https://skunity.com/dashboard/skunity-api"));
        
        Registries.Connections.Register(new RegistryKey(this, "SkriptHubConnection"),
            new ConnectionData("SkriptHub", 
                "Used as a documentation provider", 
                "SkriptHubAPIKey",
                GetIcon("SkriptHub.svg", true),
                "https://skripthub.net/dashboard/api/"));
        
        Registries.Connections.Register(new RegistryKey(this, "SkriptMCConnection"),
            new ConnectionData("SkriptMC", 
                "Used as a documentation provider", 
                "SkriptMCAPIKey",
                GetIcon("SkriptMC.png", false),
                "https://skript-mc.fr/developer/"));
        
        Registries.Connections.Register(new RegistryKey(this, "CodeSkriptPlConnection"),
            new ConnectionData("skript.pl", 
                "Used as a script host", 
                "CodeSkriptPlApiKey",
                GetIcon("skriptpl.svg", true),
                "https://code.skript.pl/api-key"));
        
        Registries.Connections.Register(new RegistryKey(this, "PastebinConnection"),
            new ConnectionData("Pastebin", 
                "Used as a script host", 
                "PastebinApiKey",
                GetIcon("Pastebin.svg", true),
                "https://pastebin.com/doc_api"));

        #endregion

        #region Registries - BottomBar

        var errors = new BottomIconData(new SymbolIconSource { Symbol = Symbol.ErrorCircle }, "errors", "0");
        var warnings = new BottomIconData(new SymbolIconSource { Symbol = Symbol.Warning }, "warnings", "0");
        var group = new BottomIconGroupData([errors, warnings], (_, args) =>
        {
            var random = new Random();
            (args.Icon as BottomIconGroupData).GetById("errors")?.UpdateText(random.Next(0, 100).ToString());
            (args.Icon as BottomIconGroupData).GetById("warnings")?.UpdateText(random.Next(0, 100).ToString());
        });
        
        var noIcon = new BottomIconData(new SymbolIconSource { Symbol = Symbol.AppFolder }, "another", null, async (_, args) =>
        {
            ((args.Icon as BottomIconData).GetIconElement().IconSource as SymbolIconSource).IsFilled = true;
            await Task.Delay(2000);
            ((args.Icon as BottomIconData).GetIconElement().IconSource as SymbolIconSource).IsFilled = false;
        });
        
        Registries.BottomIcons.Register(new RegistryKey(this, "TestIcon"), group);
        Registries.BottomIcons.Register(new RegistryKey(this, "AnotherIcon"), noIcon);

        #endregion

        #region Event Handlers

        SkEditorAPI.Events.OnFileOpened += (_, args) =>
        {
            SkEditorAPI.Logs.Debug($"File opened: {args.FilePath}, content: {args.Content}, restore? {args.CausedByRestore}");
        };

        #endregion
    }

    public List<MenuItem> GetMenuItems()
    {
        return [
            new MenuItem
            {
                Header = "SkEditor Test",
                Icon = new SymbolIcon()
                {
                    Symbol = Symbol.Attach
                }
            }];
    }
}