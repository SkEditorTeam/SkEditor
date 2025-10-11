using System;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.ViewModels;
using SkEditor.Views.Controls.Sidebar;
using ImageViewer = SkEditor.Views.Windows.FileTypes.Images.ImageViewer;

namespace SkEditor;

public class SkEditorSelfAddon : IAddon
{
    public readonly ParserSidebarPanel.ParserPanel ParserPanel = new();

    public readonly ExplorerSidebarPanel.ExplorerPanel ProjectPanel = new();


    private ImageIconSource _iconSource = null!;
    public string Name => "SkEditor Core";
    public string Version => $"{UpdateChecker.Major}.{UpdateChecker.Minor}.{UpdateChecker.Build}";
    public string Description => "The core of SkEditor, providing the base functionalities.";
    public string Identifier => "SkEditorCore";

    public IconSource GetAddonIcon()
    {
        if (_iconSource is not null)
        {
            return _iconSource;
        }

        Stream stream = AssetLoader.Open(new Uri("avares://SkEditor/Assets/SkEditor.svg"));
        return _iconSource = new ImageIconSource
            { Source = new SvgImage { Source = SvgSource.LoadFromStream(stream) } };
    }

    public void OnEnable()
    {
        #region Registries - Connections

        static IconSource GetIcon(string fileName, bool svg)
        {
            Stream stream = AssetLoader.Open(new Uri("avares://SkEditor/Assets/Brands/" + fileName));
            return new ImageIconSource
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
                    FileStream fileStream = File.OpenRead(Uri.UnescapeDataString(path));
                    Bitmap bitmap = new(fileStream);
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

    public Version GetMinimalSkEditorVersion()
    {
        return new Version(2, 7, 0);
    }
}