using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Controls.Sidebar;
using SkEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkEditor.API;

namespace SkEditor.Controls;
public partial class SideBarControl : UserControl
{
    private SidebarPanel? _currentPanel;

    public static long TransitionDuration = 100L;

    public SideBarControl()
    {
        InitializeComponent();
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
        GridPanels.Children.RemoveRange(1, GridPanels.Children.Count - 1);
        
        foreach (var panel in panels)
        {
            var btn = CreatePanelButton(panel);
            var content = panel.Content;
            content.Width = 0;
            content.Opacity = 0;

            content.Transitions = [
                new DoubleTransition()
                {
                    Property = WidthProperty,
                    Duration = TimeSpan.FromMilliseconds(TransitionDuration)
                },
                new DoubleTransition()
                {
                    Property = OpacityProperty,
                    Duration = TimeSpan.FromMilliseconds(TransitionDuration * 1.5f)
                }
            ];

            btn.Command = new RelayCommand(async () =>
            {
                if (_currentPanel == panel)
                {
                    _currentPanel.Content.Width = 0;
                    _currentPanel.Content.Opacity = 0;

                    _currentPanel.OnClose();
                    _currentPanel = null;

                    return;
                }

                if (panel.IsDisabled) return;

                if (_currentPanel != null)
                {
                    _currentPanel.OnClose();
                    _currentPanel.Content.Width = 0;
                    _currentPanel.Content.Opacity = 0;
                    _currentPanel = null;

                    await Task.Delay((int) TransitionDuration);
                }

                _currentPanel = panel;
                _currentPanel.Content.Width = _currentPanel.DesiredWidth;
                _currentPanel.Content.Opacity = 1;
                _currentPanel.OnOpen();
            });

            Grid.SetColumn(content, 1);
            GridPanels.Children.Add(content);

            Buttons.Children.Add(btn);
        }
    }

    private static Button CreatePanelButton(SidebarPanel panel)
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
        };

        return button;
    }
}
