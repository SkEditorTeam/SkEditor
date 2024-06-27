using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
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
using SkEditor.Parser.Elements;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Utilities.Parser.Elements;
using SkEditor.Utilities.Parser.Elements.Effects;
using SkEditor.ViewModels;
using SkEditor.Views;
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

        #region Registries - Margin Icons

        
        Registries.MarginIcons.Register(new RegistryKey(this, "Colors"),
            new MarginIconData(ColorIconDrawing, ColorIconClicked, "colors"));
        
        Registries.MarginIcons.Register(new RegistryKey(this, "NodeInfos"),
            new MarginIconData(NodeInfoDrawing, NodeInfoClicked, "nodes"));

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

    #region Color Icons

    public static async void ColorIconClicked(ClickedArgs clickedArgs)
    {
        var parser = clickedArgs.File["Parser"] as FileParser;
        var stack = new StackPanel() { Orientation = Orientation.Vertical, Spacing = 2 };
        var element = parser.FindNodeAtLine(clickedArgs.Line, true)?.Element as ExprProviderElement;
        if (element.Colors.Count == 1)
        {
            await SkEditorAPI.Windows.ShowWindowAsDialog(new EditColorWindow(parser, element.Colors[0]));
            return;
        }
            
        stack.Children.Add(new TextBlock() { Text = "Edit Color", FontSize = 16, FontWeight = FontWeight.SemiBold });
            
        var index = 1;
        foreach (var color in element.Colors)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5, VerticalAlignment = VerticalAlignment.Center};
                
            var avaloniaColor = new Color(color.Color.A, color.Color.R, color.Color.G, color.Color.B);
            panel.Children.Add(new Rectangle { Width = 18, Height = 18, Fill = new SolidColorBrush(avaloniaColor) });
            panel.Children.Add(new TextBlock { Text = $"Color #{index++} (as {color.Type})" });
                
            stack.Children.Add(new Button
            {
                Content = panel,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                Command = new AsyncRelayCommand(async () => 
                    await SkEditorAPI.Windows.ShowWindowAsDialog(new EditColorWindow(parser, color)))
            });
        }
            
        var flyout = new Flyout { Content = stack };
        flyout.ShowAt(SkEditorAPI.Windows.GetMainWindow(), true);
    }

    public static bool ColorIconDrawing(DrawingArgs args)
    {
        var parser = args.File["Parser"] as FileParser;
        if (parser.FindNodeAtLine(args.Line, true)?.Element is not ExprProviderElement element || element.Colors.Count == 0)
            return false;
        
        var context = args.Context;
        var y = args.Y;
        var size = 12 * args.Scale;
        var x = 8 * args.Scale - size / 2;
        
        Color ConvertColor(System.Drawing.Color color)
        {
            return new Color(color.A, color.R, color.G, color.B);
        }

        var colors = element.Colors.Select(skriptColor => skriptColor.Color).ToArray();

        if (colors.Length == 0)
            return false;
        
        if (colors.Length == 1)
        {
            var brush = new SolidColorBrush(ConvertColor(colors[0]));
            context.DrawRectangle(brush, null, new Rect(x, y, size, size));
            return true;
        }
        
        if (colors.Length == 2)
        {
            var brush1 = new SolidColorBrush(ConvertColor(colors[0]));
            var brush2 = new SolidColorBrush(ConvertColor(colors[1]));
            // Draw two triangles
            var firstTriangle = new[]
            {
                new Point(x, y),
                new Point(x + size, y),
                new Point(x, y + size)
            };
            var secondTriangle = new[]
            {
                new Point(x + size, y),
                new Point(x, y + size),
                new Point(x + size, y + size)
            };

            context.DrawGeometry(brush1, null, new PolylineGeometry(firstTriangle, true));
            context.DrawGeometry(brush2, null, new PolylineGeometry(secondTriangle, true));
            
            return true;
        }
        
        if (colors.Length == 3)
        {
            var brush1 = new SolidColorBrush(ConvertColor(colors[0]));
            var brush2 = new SolidColorBrush(ConvertColor(colors[1]));
            var brush3 = new SolidColorBrush(ConvertColor(colors[2]));
            // Draw three triangles
            var firstTriangle = new[]
            {
                new Point(x, y),
                new Point(x + size, y),
                new Point(x, y + size)
            };
            var secondTriangle = new[]
            {
                new Point(x + size, y),
                new Point(x, y + size),
                new Point(x + size, y + size)
            };
            var thirdTriangle = new[]
            {
                new Point(x, y),
                new Point(x + size, y),
                new Point(x + size, y + size)
            };

            context.DrawGeometry(brush1, null, new PolylineGeometry(firstTriangle, true));
            context.DrawGeometry(brush2, null, new PolylineGeometry(secondTriangle, true));
            context.DrawGeometry(brush3, null, new PolylineGeometry(thirdTriangle, true));
            
            return true;
        }

        if (colors.Length == 4)
        {
            var brush1 = new SolidColorBrush(ConvertColor(colors[0]));
            var brush2 = new SolidColorBrush(ConvertColor(colors[1]));
            var brush3 = new SolidColorBrush(ConvertColor(colors[2]));
            var brush4 = new SolidColorBrush(ConvertColor(colors[3]));
            
            var center = new Point(x + size / 2, y + size / 2);
            var firstTriangle = new[] { new Point(x, y), new Point(x + size, y), center };
            var secondTriangle = new[] { new Point(x + size, y), new Point(x + size, y + size), center };
            var thirdTriangle = new[] { new Point(x + size, y + size), new Point(x, y + size), center };
            var fourthTriangle = new[] { new Point(x, y + size), new Point(x, y), center };

            context.DrawGeometry(brush1, null, new PolylineGeometry(firstTriangle, true));
            context.DrawGeometry(brush2, null, new PolylineGeometry(secondTriangle, true));
            context.DrawGeometry(brush3, null, new PolylineGeometry(thirdTriangle, true));
            context.DrawGeometry(brush4, null, new PolylineGeometry(fourthTriangle, true));
            
            return true;
        }
        
        // For more than 4 colors, we draw columns of colors with equidistant spacing
        var columnWidth = size / colors.Length;
        for (var i = 0; i < colors.Length; i++)
        {
            var brush = new SolidColorBrush(ConvertColor(colors[i]));
            context.DrawRectangle(brush, null, new Rect(x + i * columnWidth, y, columnWidth, size));
        }
        
        return true;
    }

    #endregion

    #region Node Info Icons
    
    public static void NodeInfoClicked(ClickedArgs clickedArgs)
    {
        
    }
    
    public static bool NodeInfoDrawing(DrawingArgs args)
    {
        var parser = args.File["Parser"] as FileParser;
        var context = parser.LastContext;
        var drContext = args.Context;
        
        // Warnings
        foreach (var pair in context.Warnings)
        {
            var node = pair.Item1;
            var warning = pair.Item2;
            if (node.Line == args.Line)
            {
                var size = 12 * args.Scale;
                drContext.DrawImage(GetIcon("warning"), 
                    new Rect(args.X + 2, args.Y + 2, size, size));
                return true;
            }
        }

        return false;
    }

    private static IImage GetIcon(string name)
    {
        var uri = new Uri("avares://SkEditor/Assets/Icons/" + name + ".png");
        return new Bitmap(AssetLoader.Open(uri));
    }

    #endregion
}