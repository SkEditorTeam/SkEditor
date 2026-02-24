using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using SkEditor.Utilities.Files;
using SkEditor.Views.Windows;

namespace SkEditor.Utilities.Input;

public class KeyboardShortcutHandler(Window window)
{
    private WindowState _preFullScreenState = WindowState.Normal;

    public void Initialize()
    {
        window.KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        KeyboardShortcut shortcut = new(e.KeyModifiers, e.Key);

        if (TryExecuteShortcut(shortcut, e))
        {
            e.Handled = true;
        }
    }

    private bool TryExecuteShortcut(KeyboardShortcut shortcut, KeyEventArgs e)
    {
        switch (shortcut)
        {
            case { Modifiers: KeyModifiers.Control, Key: >= Key.D1 and <= Key.D9 }:
            {
                int tabIndex = (int)e.Key - 35;
                FileHandler.SwitchTab(tabIndex);
                return true;
            }
            case { Modifiers: (KeyModifiers.Control | KeyModifiers.Shift), Key: Key.Oem3 }:
                OpenTerminal();
                return true;
            case { Key: Key.F11, Modifiers: KeyModifiers.None }:
                ToggleFullScreen();
                return true;
            default:
                return false;
        }
    }

    private static void OpenTerminal()
    {
        TerminalWindow terminal = new();
        terminal.Show();
    }

    private void ToggleFullScreen()
    {
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (window.WindowState == WindowState.FullScreen)
            {
                window.WindowState = WindowState.Normal;
                await Task.Delay(10);
                window.WindowState = _preFullScreenState;
            }
            else
            {
                _preFullScreenState = window.WindowState;
                if (_preFullScreenState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                    await Task.Delay(10);
                }

                window.WindowState = WindowState.FullScreen;
            }
        });
    }

    public void Dispose()
    {
        window.KeyDown -= OnKeyDown;
    }

    public readonly record struct KeyboardShortcut(KeyModifiers Modifiers, Key Key);
}