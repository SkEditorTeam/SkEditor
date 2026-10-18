using Avalonia.Input;
using Avalonia.Interactivity;
using SkEditor.Contracts.Services.Files;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class ShortcutsService(ITabService tabs, WindowStateService windowState)
{
    public void AttachKeyEvents(MainWindow mainWindow)
    {
        mainWindow.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    private void OnKeyDown(object? _, KeyEventArgs args)
    {
        switch (args)
        {
            case { Key: >= Key.D1 and <= Key.D9 } when args.KeyModifiers.HasFlag(KeyModifiers.Control):
            {
                tabs.SwitchTab(args.Key - Key.D1);
                break;
            }

            case { Key: Key.F11 }:
            {
                windowState.ToggleFullScreen();
                break;
            }
        }
    }
}