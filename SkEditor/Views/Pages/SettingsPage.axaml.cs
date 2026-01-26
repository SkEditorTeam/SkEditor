using System;
using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace SkEditor.Views.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();

        SizeChanged += (_, e) =>
        {
            Size newSize = e.NewSize;

            double paneWidth = newSize.Width / 4;

            NavigationView.PaneDisplayMode = paneWidth < 180
                ? NavigationViewPaneDisplayMode.LeftCompact
                : NavigationViewPaneDisplayMode.Left;

            NavigationView.OpenPaneLength = Math.Min(paneWidth, 280);
        };
    }
}