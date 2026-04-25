using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class WindowStateService(StateConfig stateConfig)
{
    public void RecoverMainWindowState(MainWindow mainWindow)
    {
        if (stateConfig is { MainWindowWidth: > 0, MainWindowHeight: > 0 })
        {
            mainWindow.Width = stateConfig.MainWindowWidth;
            mainWindow.Height = stateConfig.MainWindowHeight;
        }

        PixelPoint position = new(stateConfig.MainWindowX, stateConfig.MainWindowY);
        Screens screens = mainWindow.Screens;
        PixelPoint lookupPosition = new(
            (int)(stateConfig.MainWindowX + (stateConfig.MainWindowWidth / 2)),
            (int)(stateConfig.MainWindowY + (stateConfig.MainWindowHeight / 2))
        );

        Screen? screen = screens.ScreenFromPoint(lookupPosition)
                         ?? screens.ScreenFromPoint(new PixelPoint(stateConfig.MainWindowX,
                             stateConfig.MainWindowY));

        if (screen != null)
        {
            mainWindow.Position = stateConfig.MainWindowMaximized
                ? screen.Bounds.TopLeft
                : position;
        }
        else
        {
            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        if (stateConfig.MainWindowMaximized)
        {
            mainWindow.WindowState = WindowState.Maximized;
        }
    }
}