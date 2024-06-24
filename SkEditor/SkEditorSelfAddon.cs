using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.API.Settings;
using SkEditor.API.Settings.Types;
using SkEditor.Controls.Sidebar;
using SkEditor.Parser.Elements;
using SkEditor.Utilities.Parser.Elements;
using SkEditor.Utilities.Parser.Elements.Effects;
using SkEditor.ViewModels;
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
        #region Registries - Parser Elements 
        
        Registries.ParserElements.Register(new RegistryKey(this, "StructEvent"),
            new ParserElementData(typeof(StructEvent), 1000));
        Registries.ParserElements.Register(new RegistryKey(this, "StructCommand"),
            new ParserElementData(typeof(StructCommand), 500));
        Registries.ParserElements.Register(new RegistryKey(this, "StructOptions"),
            new ParserElementData(typeof(StructOptions), 250));
        
        Registries.ParserElements.Register(new RegistryKey(this, "SecConditional"),
            new ParserElementData(typeof(SecCondition), 100));
        
        Registries.ParserElements.Register(new RegistryKey(this, "UnknownEffect"),
            new ParserElementData(typeof(UnknownEffect), 5000));

        #endregion
        
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

        #region Registries

        Registries.WelcomeEntries.Register(new RegistryKey(this, "Test1"),
            new WelcomeEntryData("Test 1", new RelayCommand(() => SkEditorAPI.Logs.Debug("Test 1 clicked")), GetAddonIcon()));

        #endregion
        
        Registries.BottomIcons.Register(new RegistryKey(this, "test1"),
            new BottomIconData()
            {
                Text = "Hello there",
                Clicked = (sender, args) =>
                {
                    SkEditorAPI.Logs.Debug("Hello there!");
                    
                    (args.Icon as BottomIconData).IsEnabled = false;
                    (args.Icon as BottomIconData).Text = "General Kenobi!";
                    (args.Icon as BottomIconData).IconSource = GetIcon("skUnity.svg", true);
                },
                IconSource = GetAddonIcon()
            });
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