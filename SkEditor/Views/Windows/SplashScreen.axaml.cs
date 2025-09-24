using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using Avalonia.Threading;

namespace SkEditor.Views.Windows;

public partial class SplashScreen : Window
{
    private readonly TextBlock _statusText;

    public SplashScreen()
    {
        InitializeComponent();

        Width = 500;
        Height = 300;
        CanResize = false;

        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = 0;
        ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        SystemDecorations = SystemDecorations.BorderOnly;

        Grid grid = new()
        {
            RowDefinitions = new RowDefinitions("*, Auto"),
            Margin = new Thickness(20)
        };

        Stream stream = AssetLoader.Open(new Uri("avares://SkEditor/Assets/SkEditor.svg"));

        Image logo = new()
        {
            Source = new SvgImage { Source = SvgSource.LoadFromStream(stream) },
            Width = 150,
            Height = 150,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        Grid.SetRow(logo, 0);

        _statusText = new TextBlock
        {
            Text = "Initializing...",
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 5, 0, 0)
        };
        Grid.SetRow(_statusText, 1);

        grid.Children.Add(logo);
        grid.Children.Add(_statusText);

        Content = grid;

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    public void UpdateStatus(string status)
    {
        Dispatcher.UIThread.Post(() => _statusText.Text = status);
    }
}