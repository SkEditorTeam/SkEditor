using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Serilog;

namespace SkEditor.Views.Windows;

public partial class SplashScreen : Window
{
    public SplashScreen()
    {
        InitializeComponent();

        Width = 500;
        Height = 300;
        CanResize = false;

        ExtendClientAreaToDecorationsHint = true;
        ExtendClientAreaTitleBarHeightHint = 0;
        WindowDecorations = WindowDecorations.BorderOnly;

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    public void UpdateStatus(string status)
    {
        Dispatcher.UIThread.Post(() => StatusText.Text = status);
    }

    public async Task UpdateStatusAsync(string status)
    {
        await Dispatcher.UIThread.InvokeAsync(() => StatusText.Text = status);
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