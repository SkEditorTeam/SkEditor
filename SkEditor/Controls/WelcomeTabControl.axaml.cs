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
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.Avalonia.Fluent.SymbolIcon;

namespace SkEditor.Controls;

public partial class WelcomeTabControl : UserControl
{
    private const double IconSize = 24;
    private const int TitleSize = 28;
    private const int TextSize = 16;

    // This is just a little, funny thing serving no real purpose... But it's nice, right?
    private readonly string[] _tooltips = ["Hey!", "Hi!", "How are you?", "What will you code today?"];

    public WelcomeTabControl()
    {
        InitializeComponent();

        var gettingStarted = CreateActionSection(Translation.Get("WelcomeTabGettingStarted"),
        [
            new WelcomeEntryData(Translation.Get("WelcomeGettingStartedNewFile"), new RelayCommand(FileHandler.NewFile), CreateSymbolIcon(Symbol.DocumentAdd)),
            new WelcomeEntryData(Translation.Get("WelcomeGettingStartedOpenFile"), new AsyncRelayCommand(FileHandler.OpenFile), CreateSymbolIcon(Symbol.DocumentSearch)),
            new WelcomeEntryData(Translation.Get("WelcomeGettingStartedOpenFolder"), new RelayCommand(() => ProjectOpener.OpenProject()), CreateSymbolIcon(Symbol.FolderOpen)),
            new WelcomeEntryData(Translation.Get("WelcomeGettingStartedSettings"), new AsyncRelayCommand(OpenSettings), CreateSymbolIcon(Symbol.Settings)),
        ]);

        var help = CreateActionSection(Translation.Get("WelcomeTabNeedHelp"),
        [
            new WelcomeEntryData(Translation.Get("WelcomeNeedHelpDiscordServer"), new RelayCommand(() =>
                SkEditorAPI.Core.OpenLink("https://skeditordc.notro.me/")), CreatePathIconSource("DiscordIcon")),

            new WelcomeEntryData(Translation.Get("WelcomeNeedHelpGitHub"), new RelayCommand(() =>
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
        FontSize = IconSize
    };

    private static PathIconSource CreatePathIconSource(string name) => SkEditorAPI.Core.GetApplicationResource(name) as PathIconSource;

    private void SetupGrid(List<StackPanel?> panels)
    {
        var x = 0;
        var y = 0;
        foreach (StackPanel panel in panels.OfType<StackPanel>())
        {
            WelcomeGrid.Children.Add(panel);
            Grid.SetRow(panel, y);
            Grid.SetColumn(panel, x);

            x++;
            if (x != 2) continue;

            x = 0;
            y++;
        }
    }

    private static StackPanel? CreateAddonsSection()
    {
        var addonsEntries = Registries.WelcomeEntries.ToList();
        return addonsEntries.Count == 0 ? null : CreateActionSection("Addons", addonsEntries);
    }

    public static StackPanel CreateActionSection(string name, List<WelcomeEntryData> entries)
    {
        var panel = new StackPanel { Spacing = 5 };
        var title = new TextBlock { Text = name, FontSize = TitleSize, FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 0, 0, 5) };
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

            var textBlock = new TextBlock { Text = entry.Name, FontSize = TextSize };
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };

            switch (entry.Icon)
            {
                case SymbolIcon symbolIcon:
                    buttonPanel.Children.Add(symbolIcon);
                    break;
                case IconSource iconSource:
                    buttonPanel.Children.Add(new IconSourceElement
                    {
                        IconSource = iconSource,
                        Width = IconSize,
                        Height = IconSize,
                    });
                    break;
            }
            buttonPanel.Children.Add(textBlock);

            button.Content = buttonPanel;
            panel.Children.Add(button);
        }

        return panel;
    }

    private static async Task OpenSettings() => await new SettingsWindow().ShowDialog(SkEditorAPI.Windows.GetMainWindow());
}