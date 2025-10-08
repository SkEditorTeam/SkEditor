using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Projects;
using SettingsWindow = SkEditor.Views.Windows.Settings.SettingsWindow;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.Avalonia.Fluent.SymbolIcon;

namespace SkEditor.Views.Controls;

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

        StackPanel gettingStarted = CreateActionSection(Translation.Get("WelcomeTabGettingStarted"),
        [
            new WelcomeEntryData(Translation.Get("WelcomeGettingStartedNewFile"), new RelayCommand(FileHandler.NewFile),
                CreateSymbolIcon(Symbol.DocumentAdd)),
            new WelcomeEntryData(Translation.Get("WelcomeGettingStartedOpenFile"),
                new AsyncRelayCommand(FileHandler.OpenFile), CreateSymbolIcon(Symbol.DocumentSearch)),
            new WelcomeEntryData(Translation.Get("WelcomeGettingStartedOpenFolder"),
                new AsyncRelayCommand(async () => await ProjectOpener.OpenProject()),
                CreateSymbolIcon(Symbol.FolderOpen)),
            new WelcomeEntryData(Translation.Get("WelcomeGettingStartedSettings"), new AsyncRelayCommand(OpenSettings),
                CreateSymbolIcon(Symbol.Settings))
        ]);

        StackPanel help = CreateActionSection(Translation.Get("WelcomeTabNeedHelp"),
        [
            new WelcomeEntryData(Translation.Get("WelcomeNeedHelpDiscordServer"), new RelayCommand(() =>
                SkEditorAPI.Core.OpenLink("https://skeditordc.notro.me/")), CreatePathIconSource("DiscordIcon")),

            new WelcomeEntryData(Translation.Get("WelcomeNeedHelpGitHub"), new RelayCommand(() =>
                SkEditorAPI.Core.OpenLink("https://github.com/SkEditorTeam/SkEditor")),
                CreatePathIconSource("GitHubIcon")),
            
            new WelcomeEntryData(Translation.Get("WelcomeNeedHelpDocumentation"),
                new RelayCommand(() => SkEditorAPI.Core.OpenLink("https://docs.skeditor.dev")),
                CreateSymbolIcon(Symbol.Book))
        ]);

        StackPanel? addons = CreateAddonsSection();

        SetupGrid([gettingStarted, help, addons]);
        VersionText.Text = $"v{SkEditorAPI.Core.GetInformationalVersion()}";

        ToolTip.SetShowDelay(SkEditorIcon, 500);
        SkEditorIcon.PointerEntered += (_, _) =>
        {
            int index = new Random().Next(_tooltips.Length);
            ToolTip.SetTip(SkEditorIcon, _tooltips[index]);
        };
    }

    private static SymbolIcon CreateSymbolIcon(Symbol symbol)
    {
        return new SymbolIcon
        {
            Symbol = symbol,
            FontSize = IconSize
        };
    }

    private static PathIconSource? CreatePathIconSource(string name)
    {
        return SkEditorAPI.Core.GetApplicationResource(name) as PathIconSource;
    }

    private void SetupGrid(List<StackPanel?> panels)
    {
        int x = 0;
        int y = 0;
        foreach (StackPanel panel in panels.OfType<StackPanel>())
        {
            WelcomeGrid.Children.Add(panel);
            Grid.SetRow(panel, y);
            Grid.SetColumn(panel, x);

            x++;
            if (x != 2)
            {
                continue;
            }

            x = 0;
            y++;
        }
    }

    private static StackPanel? CreateAddonsSection()
    {
        List<WelcomeEntryData> addonsEntries = Registries.WelcomeEntries.ToList();
        return addonsEntries.Count == 0 ? null : CreateActionSection("Addons", addonsEntries);
    }

    public static StackPanel CreateActionSection(string name, List<WelcomeEntryData> entries)
    {
        StackPanel panel = new() { Spacing = 5 };
        TextBlock title = new()
            { Text = name, FontSize = TitleSize, FontWeight = FontWeight.SemiBold, Margin = new Thickness(0, 0, 0, 5) };
        panel.Children.Add(title);

        foreach (WelcomeEntryData entry in entries)
        {
            Button button = new()
            {
                Command = entry.Command,
                Padding = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };

            TextBlock textBlock = new() { Text = entry.Name, FontSize = TextSize };
            StackPanel buttonPanel = new() { Orientation = Orientation.Horizontal, Spacing = 10 };

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
                        Height = IconSize
                    });
                    break;
            }

            buttonPanel.Children.Add(textBlock);

            button.Content = buttonPanel;
            panel.Children.Add(button);
        }

        return panel;
    }

    private static async Task OpenSettings()
    {
        await new SettingsWindow().ShowDialogOnMainWindow();
    }
}