using Avalonia.Input;
using Avalonia.Interactivity;
using SkEditor.Contracts.Services.Files;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class ShortcutsService(ITabService tabs)
{
    public void AttachKeyEvents(MainWindow mainWindow)
    {
        mainWindow.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    private void OnKeyDown(object? _, KeyEventArgs args)
    {
        switch (args)
        {
            case { KeyModifiers: KeyModifiers.Control, Key: >= Key.D1 and <= Key.D9 }:
                tabs.SwitchTab(args.Key - Key.D1);
                break;
        }
    }
}