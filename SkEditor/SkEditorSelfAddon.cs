using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.API.Settings;
using SkEditor.API.Settings.Types;
using SkEditor.Controls.Sidebar;
using SkEditor.ViewModels;
using SkEditor.Views.FileTypes;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.Avalonia.Fluent.SymbolIcon;

namespace SkEditor;

public class SkEditorSelfAddon : IAddon
{
    public string Name => "SkEditor Core";
    public string Version => SettingsViewModel.Version;
    public string Description => "The core of SkEditor, providing the base functionalities.";
    public string Identifier => "SkEditorCore";
    
    public readonly ExplorerSidebarPanel.ExplorerPanel ProjectPanel = new();
    public readonly ParserSidebarPanel.ParserPanel ParserPanel = new();
    

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

        static IconSource GetIcon(string fileName, bool svg)
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

        #region Registries - Sidebar Panels
        
        Registries.SidebarPanels.Register(new RegistryKey(this, "ProjectPanel"), ProjectPanel);
        Registries.SidebarPanels.Register(new RegistryKey(this, "ParserPanel"), ParserPanel);

        #endregion

        #region Registries - File Types

        Registries.FileTypes.Register(new RegistryKey(this, "Images"), new FileTypeData(
            [".png", ".jpg", ".ico", ".jpeg", ".gif", ".bmp", ".tiff", ".webp"],
            path =>
            {
                try
                {
                    var fileStream = File.OpenRead(Uri.UnescapeDataString(path));
                    var bitmap = new Bitmap(fileStream);
                    fileStream.Close();

                    return new FileTypeResult(new ImageViewer(bitmap, path), Path.GetFileNameWithoutExtension(path));
                }
                catch (Exception e)
                {
                    SkEditorAPI.Windows.ShowError($"Unable to load the specified image:\n\n{e.Message}");
                    return null;
                }
            }));

        #endregion
    }

    public List<MenuItem> GetMenuItems()
    {
        return [new MenuItem
        {
            Header = "SkEditor Test",
            Icon = new SymbolIcon()
            {
                Symbol = Symbol.Attach
            },
            Command = new RelayCommand(() => SkEditorAPI.Addons.DisableAddon(this))
        }];
    }

    public Version GetMinimalSkEditorVersion()
    {
        return new Version(2, 5, 0);
    }

    public List<Setting> GetSettings()
    {
        return
        [
            
            new Setting(this, "TestSetting", "TestSettingKey", true,
                new ToggleSetting(), "This is a test setting!", GetAddonIcon()) {
                OnChanged = value =>
                {
                    SkEditorAPI.Logs.Debug($"Test setting changed to {value}");
                }
            },
            
            new Setting(this, "TestStringSetting", "TestStringSetting", "",
                new TextSetting("Enter something here")) {
                OnChanged = value =>
                {
                    SkEditorAPI.Logs.Debug($"his password is {value}");
                }
            }

        ];
    }
}