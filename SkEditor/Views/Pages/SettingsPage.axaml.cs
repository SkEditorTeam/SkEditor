using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;

namespace SkEditor.Views.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();

        Loaded += OnLoaded;
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
            ? FANavigationViewPaneDisplayMode.LeftCompact
            : FANavigationViewPaneDisplayMode.Left;

        NavigationView.OpenPaneLength = Math.Min(paneWidth, 280);
    }
}