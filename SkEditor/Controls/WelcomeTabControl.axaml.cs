using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Projects;
using SkEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.Avalonia.Fluent.SymbolIcon;

namespace SkEditor.Controls;

public partial class WelcomeTabControl : UserControl
{
    private const double _iconSize = 28;
    private const int _titleSize = 28;
    private const int _textSize = 16;

    // This is just a little, funny thing serving no real purpose... But it's nice, right?
    private readonly string[] _tooltips = ["Hey!", "Hi!", "How are you?", "This addon rework is kinda cool, right?", "Ohh, a pre-release? Nice!", "What will you code today?"];

    public WelcomeTabControl()
    {
        InitializeComponent();

        var gettingStarted = CreateActionSection(Translation.Get("WelcomeTabGettingStarted"),
        [
            new (Translation.Get("WelcomeGettingStartedNewFile"), new (FileHandler.NewFile), CreateSymbolIcon(Symbol.DocumentAdd)),
            new (Translation.Get("WelcomeGettingStartedOpenFile"), new (FileHandler.OpenFile), CreateSymbolIcon(Symbol.DocumentSearch)),
            new (Translation.Get("WelcomeGettingStartedOpenFolder"), new (() => ProjectOpener.OpenProject()), CreateSymbolIcon(Symbol.FolderOpen)),
            new (Translation.Get("WelcomeGettingStartedSettings"), new (OpenSettings), CreateSymbolIcon(Symbol.Settings)),
        ]);

        var help = CreateActionSection(Translation.Get("WelcomeTabNeedHelp"),
        [
            new (Translation.Get("WelcomeNeedHelpDiscordServer"), new (() =>
                SkEditorAPI.Core.OpenLink("https://skeditordc.notro.me/")), CreatePathIconSource("DiscordIcon")),

            new (Translation.Get("WelcomeNeedHelpGitHub"), new (() =>
                SkEditorAPI.Core.OpenLink("https://github.com/SkEditorTeam/SkEditor")), CreatePathIconSource("GitHubIcon")),
        ]);

        var addons = CreateAddonsSection();

        SetupGrid([gettingStarted, help, addons]);
        VersionText.Text = $"v{SkEditorAPI.Core.GetInformationalVersion()}";

        ToolTip.SetShowDelay(SkEditorIcon, 500);
        SkEditorIcon.PointerEntered += (_, _) =>
        {
            int index = new Random().Next(_tooltips.Length);
            ToolTip.SetTip(SkEditorIcon, _tooltips[index]);
        };
    }

    private static SymbolIcon CreateSymbolIcon(Symbol symbol) => new()
    {
        Symbol = symbol,
        FontSize = _iconSize
    };

    private static PathIconSource CreatePathIconSource(string name) => SkEditorAPI.Core.GetApplicationResource(name) as PathIconSource;

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

    private static StackPanel? CreateAddonsSection()
    {
        var addonsEntries = Registries.WelcomeEntries.ToList();
        if (addonsEntries.Count == 0)
            return null;

        return CreateActionSection("Addons", addonsEntries);
    }

    public static StackPanel CreateActionSection(string name, List<WelcomeEntryData> entries)
    {
        var panel = new StackPanel { Spacing = 5 };
        var title = new TextBlock { Text = name, FontSize = _titleSize, FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 0, 0, 5) };
        panel.Children.Add(title);

        foreach (var entry in entries)
        {
            var button = new Button
            {
                Command = entry.Command,
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };

            var textBlock = new TextBlock { Text = entry.Name, FontSize = _textSize };
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };

            if (entry.Icon is SymbolIcon symbolIcon)
            {
                buttonPanel.Children.Add(symbolIcon);
            }
            else if (entry.Icon is IconSource iconSource)
            {
                buttonPanel.Children.Add(new IconSourceElement
                {
                    IconSource = iconSource,
                    Width = _iconSize,
                    Height = _iconSize,
                });
            }
            buttonPanel.Children.Add(textBlock);

            button.Content = buttonPanel;
            panel.Children.Add(button);
        }

        return panel;
    }

    private async void OpenSettings() => await new SettingsWindow().ShowDialog(SkEditorAPI.Windows.GetMainWindow());
}