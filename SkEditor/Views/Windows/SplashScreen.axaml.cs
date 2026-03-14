using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using Serilog;

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
        SvgImage svgImage = new() { Source = SvgSource.LoadFromStream(stream) };

        Image logo = new()
        {
            Source = svgImage,
            Width = 150,
            Height = 150,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Effect = new DropShadowEffect
            {
                Color = Color.Parse("#64000000"),
                BlurRadius = 15,
                OffsetX = 0,
                OffsetY = 5
            }
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

    public async Task UpdateStatusAsync(string status)
    {
        await Dispatcher.UIThread.InvokeAsync(() => _statusText.Text = status);
    }
}

internal static class SplashScreenExtensions
{
    public static Task TryUpdateStatusAsync(this SplashScreen? splashScreen, string status)
    {
        if (splashScreen == null)
        {
            return Task.CompletedTask;
        }

        try
        {
            return splashScreen.UpdateStatusAsync(status);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Error updating splash screen");
            return Task.CompletedTask;
        }
    }
}