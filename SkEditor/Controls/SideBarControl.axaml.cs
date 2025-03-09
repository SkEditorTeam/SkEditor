using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;
using Symbol = FluentIcons.Common.Symbol;
using Serilog;

namespace SkEditor.Controls;
public partial class SideBarControl : UserControl
{
    private SidebarPanel? _currentPanel;

    public static long TransitionDuration = 100L;

    private SolidColorBrush _buttonForeground = new(Color.Parse("#cccccc"));
    private SolidColorBrush _activeButtonForeground = new(Color.Parse("#60cdff"));

    public SideBarControl()
    {
        InitializeComponent();

        Loaded += (_, _) => SkEditorAPI.Windows.GetMainWindow().Splitter.DragCompleted += (sender, args) =>
        {
            if (_currentPanel == null)
                return;

            SkEditorAPI.Core.GetAppConfig().SidebarPanelSizes[_currentPanel.GetId()] =
                (int)SkEditorAPI.Windows.GetMainWindow().CoreGrid.ColumnDefinitions[1].Width.Value;
        };
    }

    public void ReloadPanels()
    {
        var panels = Registries.SidebarPanels;
        if (!panels.Any())
        {
            IsVisible = false;
            return;
        }

        IsVisible = true;
        Buttons.Children.Clear();
        SkEditorAPI.Windows.GetMainWindow().SidebarContentBorder.Child = null;
        _currentPanel = null;

        foreach (var panel in panels)
            Buttons.Children.Add(CreatePanelButton(panel));
    }

    private Button CreatePanelButton(SidebarPanel panel)
    {
        var icon = panel.Icon;
        var activeIcon = panel.IconActive;

        Viewbox iconViewBox = CreateIconViewbox(icon, _buttonForeground);
        Viewbox activeIconViewBox = CreateIconViewbox(activeIcon, _activeButtonForeground);

        Button button = new()
        {
            Height = 48,
            Width = 48,
            Classes = { "barButton" },
            Content = iconViewBox,
            IsEnabled = !panel.IsDisabled,
            Tag = (iconViewBox, activeIconViewBox, panel)
        };

        button.Command = new RelayCommand(() =>
        {
            foreach (var child in Buttons.Children)
            {
                if (child is Button btn && btn.Tag is ValueTuple<Viewbox, Viewbox, SidebarPanel> buttonData)
                {
                    var (defaultIcon, _, btnPanel) = buttonData;
                    btn.Content = defaultIcon;
                }
            }

            if (_currentPanel == panel)
            {
                _currentPanel.OnClose();
                _currentPanel = null;
                SkEditorAPI.Windows.GetMainWindow().SidebarContentBorder.Child = null;

                SkEditorAPI.Windows.GetMainWindow().CoreGrid.ColumnDefinitions[1].MaxWidth = 0;
                SkEditorAPI.Windows.GetMainWindow().CoreGrid.ColumnDefinitions[1].MinWidth = 0;

                SkEditorAPI.Windows.GetMainWindow().SideBar.Margin = new Thickness(0, 0, 0, 0);

                return;
            }

            if (panel.IsDisabled)
                return;

            if (_currentPanel != null)
            {
                _currentPanel.OnClose();
                _currentPanel = null;
            }

            if (button.Tag is ValueTuple<Viewbox, Viewbox, SidebarPanel> buttonData2)
            {
                var (_, activeIcon, _) = buttonData2;
                button.Content = activeIcon;
            }

            _currentPanel = panel;
            _currentPanel.OnOpen();
            SkEditorAPI.Windows.GetMainWindow().SidebarContentBorder.Child = _currentPanel.Content;

            var configuredWidth = SkEditorAPI.Core.GetAppConfig().SidebarPanelSizes.GetValueOrDefault(_currentPanel.GetId(), _currentPanel.DesiredWidth);
            SkEditorAPI.Windows.GetMainWindow().CoreGrid.ColumnDefinitions[1].Width = new GridLength(configuredWidth);

            SkEditorAPI.Windows.GetMainWindow().CoreGrid.ColumnDefinitions[1].MinWidth = _currentPanel.DesiredWidth;
            SkEditorAPI.Windows.GetMainWindow().CoreGrid.ColumnDefinitions[1].MaxWidth = int.MaxValue;

            SkEditorAPI.Windows.GetMainWindow().SideBar.Margin = new Thickness(0, 0, 10, 0);
        });

        return button;
    }

    private static Viewbox CreateIconViewbox(IconSource icon, SolidColorBrush foreground)
    {
        var iconElement = new IconSourceElement()
        {
            IconSource = icon,
            Foreground = foreground,
        };

        return new Viewbox
        {
            Child = iconElement,
            Width = 26,
            Height = 26
        };
    }
}
