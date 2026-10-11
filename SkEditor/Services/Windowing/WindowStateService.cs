using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using MemoryPack;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Session;
using SkEditor.Utilities;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class WindowStateService(
    AppSettings appSettings,
    StateConfig stateConfig,
    WindowRegistry windowRegistry,
    ILogService logs,
    IFileTabFactory fileTabFactory,
    ITabService tabs)
{
    private WindowState _preFullScreenState = WindowState.Normal;

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

        bool isSessionRestoreEnabled = appSettings.IsSessionRestoreEnabled;
        if (isSessionRestoreEnabled)
        {
            RecoverSessionState();
        }
    }

    private void RecoverSessionState()
    {
        string path = Path.Combine(AppPaths.LocalData, "session.bin");
        if (!File.Exists(path))
        {
            return;
        }

        byte[] bytes = File.ReadAllBytes(path);
        SessionState? state = MemoryPackSerializer.Deserialize<SessionState>(bytes);
        if (state is null)
        {
            return;
        }

        logs.Debug(state.ToString());

        foreach (SessionStateFile file in state.Files)
        {
            FileTabViewModel tab = fileTabFactory.CreateFromSession(file);

            if (file.Content is not null)
            {
                tab.CaretOffset = file.CaretOffset;
                tab.Document.Text = file.Content;
                tab.Document.UndoStack.ClearAll();
            }

            tabs.OpenTab(tab);
        }
    }

    public void ToggleFullScreen()
    {
        if (windowRegistry.Active is not { } window)
        {
            return;
        }

        if (window.WindowState == WindowState.FullScreen)
        {
            window.WindowState = _preFullScreenState;
        }
        else
        {
            _preFullScreenState = window.WindowState;
            window.WindowState = WindowState.FullScreen;
        }
    }
}