using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Projects;
using SkEditor.Views;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Controls;

public partial class WelcomeTabControl : UserControl
{
    public WelcomeTabControl()
    {
        InitializeComponent();

        var gettingStarted = CreateActionSection("Getting Started", [
            new ("New File", new (FileHandler.NewFile), new SymbolIconSource() { Symbol = Symbol.DocumentAdd }),
            new ("Open File", new (FileHandler.OpenFile), new SymbolIconSource() { Symbol = Symbol.DocumentSearch }),
            new ("Open Folder", new (() => ProjectOpener.OpenProject()), new SymbolIconSource() { Symbol = Symbol.FolderOpen }),
            new ("Settings", new (OpenSettings), new SymbolIconSource() { Symbol = Symbol.Settings }),
        ]);
        
        var help = CreateActionSection("Need Help?", [
            new ("Discord Server", new (() => SkEditorAPI.Core.OpenLink("https://skeditordc.notro.me/")), SkEditorAPI.Core.GetApplicationResource("DiscordIcon") as PathIconSource),
            new ("GitHub", new (() => SkEditorAPI.Core.OpenLink("https://github.com/SkEditorTeam/SkEditor")), SkEditorAPI.Core.GetApplicationResource("GitHubIcon") as PathIconSource),
            new ("skUnity Resource", new (() => SkEditorAPI.Core.OpenLink("https://forums.skunity.com/resources/1517/")), SkEditorAPI.Core.GetApplicationResource("SkEditorIcon") as PathIconSource)
        ]);
        
        var addons = CreateAddonsSection();
        
        SetupGrid([gettingStarted, help, addons]);
    }

    private void SetupGrid(List<StackPanel?> panels)
    {
        var x = 0;
        var y = 0;
        foreach (var panel in panels)
        {
            if (panel == null)
                continue;
            
            WelcomeGrid.Children.Add(panel);
            Grid.SetRow(panel, y);
            Grid.SetColumn(panel, x);
            
            x++; 
            if (x == 2) { x = 0; y++; }
        }
    }

    private StackPanel? CreateAddonsSection()
    {
        var addonsEntries = Registries.WelcomeEntries.ToList();
        if (addonsEntries.Count == 0)
            return null;
        
        return CreateActionSection("Addons", addonsEntries);
    }

    public StackPanel CreateActionSection(string name, List<WelcomeEntryData> entries)
    {
        var panel = new StackPanel { Spacing = 5 };
        var title = new TextBlock { Text = name, FontSize = 28, FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 0, 0, 5)};
        panel.Children.Add(title);
        
        foreach (var entry in entries)
        {
            var button = new Button { Command = entry.Command, Padding = new Thickness(5)};
            var textBlock = new TextBlock { Text = entry.Name };
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 5 };
            
            if (entry.Icon != null)
                buttonPanel.Children.Add(new IconSourceElement
                {
                    IconSource = entry.Icon,
                    Width = 20,
                    Height = 20
                });
            buttonPanel.Children.Add(textBlock);
            
            button.Content = buttonPanel;
            panel.Children.Add(button);
        }
        
        return panel;
    }

    private async void OpenSettings()
    {
        await new SettingsWindow().ShowDialog(SkEditorAPI.Windows.GetMainWindow());
    }
}