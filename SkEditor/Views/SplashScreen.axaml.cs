using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.IO;
using Avalonia.Platform;
using Avalonia.Svg.Skia;

namespace SkEditor.Views;

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

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("*, Auto"),
            Margin = new Thickness(20)
        };

        Stream stream = AssetLoader.Open(new Uri("avares://SkEditor/Assets/SkEditor.svg"));

        var logo = new Image
        {
            Source = new SvgImage { Source = SvgSource.LoadFromStream(stream) },
            Width = 150,
            Height = 150,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
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