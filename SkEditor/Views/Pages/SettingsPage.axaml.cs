using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using AvaloniaEdit.Utils;
using FluentAvalonia.UI.Controls;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Views.Pages;

public partial class SettingsPage : UserControl
{
    public Dictionary<int, List<SettingElement>> SettingsSections { get; } = new()
    {
        {
            0, [
                new ToggleSetting(
                    "Enable Auto-Save",
                    new SymbolIconSource { Symbol = Symbol.Save },
                    "Automatically save files at regular intervals.",
                    true),
                new ActionSetting(
                    "Manage Plugins",
                    new SymbolIconSource { Symbol = Symbol.PuzzlePiece },
                    "Add, remove, or update plugins for additional functionality.",
                    "Open Plugin Manager")
            ]
        },
        {
            1, [
                new ToggleSetting(
                    "Enable Dark Mode",
                    new SymbolIconSource { Symbol = Symbol.Color },
                    "Switch the application theme to dark mode.",
                    false),
                new ActionSetting(
                    "Customize Theme",
                    new SymbolIconSource { Symbol = Symbol.PaintBrush },
                    "Open the theme customization settings.",
                    "Open Theme Settings")
            ]
        }
    };

    public ObservableCollection<SettingElement> CurrentSection { get; } = [];

    public SettingsPage()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        NavigationView.SelectionChanged += (_, _) =>
        {
            int index = NavigationView.MenuItems.IndexOf(NavigationView.SelectedItem);
            if (!SettingsSections.TryGetValue(index, out List<SettingElement>? section))
                return;

            CurrentSection.Clear();
            CurrentSection.AddRange(section);
        };
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        NavigationView.SizeChanged += (_, ev) => UpdateViewSize(ev.NewSize);

        UpdateViewSize(NavigationView.Bounds.Size);
        ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
    }

    private void UpdateViewSize(Size paneSize)
    {
        double paneWidth = paneSize.Width / 4;

        NavigationView.PaneDisplayMode = paneWidth < 180
            ? NavigationViewPaneDisplayMode.LeftCompact
            : NavigationViewPaneDisplayMode.Left;

        NavigationView.OpenPaneLength = Math.Min(paneWidth, 280);
    }
}

public abstract class SettingElement(string name, IconSource icon, string description)
{
    public string Name { get; set; } = name;
    public IconSource Icon { get; set; } = icon;
    public string Description { get; set; } = description;
}

public class ToggleSetting(string name, IconSource icon, string description, bool isEnabled)
    : SettingElement(name, icon, description)
{
    public bool IsEnabled { get; set; } = isEnabled;
}

public class ActionSetting(string name, IconSource icon, string description, string buttonText) 
    : SettingElement(name, icon, description)
{
    public string ButtonText { get; set; } = buttonText;
}