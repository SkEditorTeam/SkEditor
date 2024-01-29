using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Controls.Sidebar;
using SkEditor.Utilities;

namespace SkEditor.Controls;
public partial class SideBarControl : UserControl
{
    private static readonly List<SidebarPanel> Panels = new();
    
    public readonly ExplorerSidebarPanel.ExplorerPanel ProjectPanel = new();
    public readonly ParserSidebarPanel.ParserPanel ParserPanel = new();
    
    public static void RegisterPanel(SidebarPanel panel)
    {
        Panels.Add(panel);
    }
    
    private SidebarPanel? _currentPanel;
    public SideBarControl()
    {
        InitializeComponent();
        
        RegisterPanel(ProjectPanel);
        RegisterPanel(ParserPanel);
    }

    public static long TransitionDuration = 100L;
    public async void LoadPanels()
    {
        foreach (SidebarPanel panel in Panels)
        {
            var btn = CreatePanelButton(panel);
            var content = panel.Content;
            content.Width = 0;
            content.Opacity = 0;

            content.Transitions = new Transitions() { 
                new DoubleTransition()
                {
                    Property = WidthProperty, 
                    Duration = TimeSpan.FromMilliseconds(TransitionDuration)
                }, 
                new DoubleTransition()
                {
                    Property = OpacityProperty, 
                    Duration = TimeSpan.FromMilliseconds(150)
                }
            };
            
            btn.Command = new RelayCommand(async () =>
            {
                if (_currentPanel == panel)
                {
                    _currentPanel.Content.Width = 0; // Close current panel
                    _currentPanel.Content.Opacity = 0;
                    
                    _currentPanel.OnClose();
                    _currentPanel = null;
                    
                    return;
                }
                
                if (panel.IsDisabled) 
                    return;
                
                if (_currentPanel != null)
                {
                    _currentPanel.OnClose();
                    _currentPanel.Content.Width = 0; // Close current panel
                    _currentPanel.Content.Opacity = 0;
                    _currentPanel = null;
                    
                    await Task.Delay((int) TransitionDuration);
                }
                
                _currentPanel = panel;
                _currentPanel.Content.Width = _currentPanel.DesiredWidth;
                _currentPanel.Content.Opacity = 1;
                _currentPanel.OnOpen();
            });
            
            GridPanels.Children.Add(content);
            Grid.SetColumn(content, 1);
            
            Buttons.Children.Add(btn);
        }
    }

    private Button CreatePanelButton(SidebarPanel panel)
    {
        var icon = panel.Icon;
        if (icon is SymbolIconSource symbolIcon) symbolIcon.FontSize = 24;
        IconSourceElement iconElement = new()
        {
            IconSource = icon,
            Width = 24,
            Height = 24,
            Foreground = new SolidColorBrush(Color.Parse("#bfffffff")),
        };
        
        Button button = new()
        {
            Height = 36,
            Width = 36,
            Classes = { "barButton" },
            Content = iconElement,
            IsEnabled = !panel.IsDisabled,
        };
        
        return button;
    }
}
