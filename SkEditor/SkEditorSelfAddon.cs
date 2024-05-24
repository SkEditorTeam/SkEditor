using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.API.Model;
using SkEditor.API.Registry;
using SkEditor.ViewModels;

namespace SkEditor;

public class SkEditorSelfAddon : IAddon
{
    public string Name => "SkEditorCore";
    public string Version => SettingsViewModel.Version;
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
    }
}