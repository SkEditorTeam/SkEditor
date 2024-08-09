using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace SkEditor.Controls;
public partial class SideBarControl : UserControl
{
    private SidebarPanel? _currentPanel;

    public static long TransitionDuration = 100L;

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
        if (icon is SymbolIconSource symbolIcon) symbolIcon.FontSize = 24;
        IconSourceElement iconElement = new()
        {
            IconSource = icon,
            Width = 36,
            Height = 36,
            Foreground = new SolidColorBrush(Color.Parse("#bfffffff")),
        };

        Button button = new()
        {
            Height = 42,
            Width = 42,
            Classes = { "barButton" },
            Content = iconElement,
            IsEnabled = !panel.IsDisabled,
            Command = new RelayCommand(() =>
            {
                if (_currentPanel == panel)
                {
                    _currentPanel.OnClose();
                    _currentPanel = null;
                    SkEditorAPI.Windows.GetMainWindow().SidebarContentBorder.Child = null;

                    SkEditorAPI.Windows.GetMainWindow().CoreGrid.ColumnDefinitions[1].MaxWidth = 0;
                    SkEditorAPI.Windows.GetMainWindow().CoreGrid.ColumnDefinitions[1].MinWidth = 0;

                    return;
                }

                if (panel.IsDisabled)
                    return;

                if (_currentPanel != null)
                {
                    _currentPanel.OnClose();
                    _currentPanel = null;
                }

                _currentPanel = panel;
                _currentPanel.OnOpen();
                SkEditorAPI.Windows.GetMainWindow().SidebarContentBorder.Child = _currentPanel.Content;

                var configuredWidth = SkEditorAPI.Core.GetAppConfig().SidebarPanelSizes.GetValueOrDefault(_currentPanel.GetId(), _currentPanel.DesiredWidth);
                SkEditorAPI.Windows.GetMainWindow().CoreGrid.ColumnDefinitions[1].Width = new GridLength(configuredWidth);

                SkEditorAPI.Windows.GetMainWindow().CoreGrid.ColumnDefinitions[1].MinWidth = _currentPanel.DesiredWidth;
                SkEditorAPI.Windows.GetMainWindow().CoreGrid.ColumnDefinitions[1].MaxWidth = int.MaxValue;
            })
        };

        return button;
    }
}
