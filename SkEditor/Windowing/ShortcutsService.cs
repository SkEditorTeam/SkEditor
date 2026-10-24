using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using R3;
using SkEditor.Windowing.UI;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Windowing;

public class ShortcutsService(ITabService tabs, WindowStateService windowState, WindowRegistry windowRegistry)
{
    private IInputElement? _lastNonMenuFocusedElement;
    
    public void AttachKeyEvents(MainWindow mainWindow)
    {
        mainWindow.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);

        InputElement.GotFocusEvent.Raised.ToObservable().Subscribe(e =>
        {
            if (e.Item2.Source is not (IInputElement element and not (MenuItem or Menu)))
            {
                return;
            }
            _lastNonMenuFocusedElement = element;
        });
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

            case { Key: Key.Escape }:
            {
                HandleFocus(args);
                break;
            }
        }
    }

    private void HandleFocus(KeyEventArgs args)
    {
        IFocusManager? focusManager = TopLevel.GetTopLevel(windowRegistry.Active)?.FocusManager;
        IInputElement? element = focusManager?.GetFocusedElement();
        
        if (element is not (MenuItem { IsTopLevel: true } or Menu))
        {
            return;
        }

        if (_lastNonMenuFocusedElement is { IsEffectivelyVisible: true, IsEffectivelyEnabled: true })
        {
            focusManager?.Focus(_lastNonMenuFocusedElement);
        }
        else
        {
            if (tabs.SelectedTab is IFocusableTab focusableTab)
            {
                focusableTab.RequestFocus();
            }
        }

        args.Handled = true;
    }
}