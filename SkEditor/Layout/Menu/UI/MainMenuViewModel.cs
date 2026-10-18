using System.Collections.Generic;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Layout.Menu.Models;
using SkEditor.Windowing.UI;

namespace SkEditor.Layout.Menu.UI;

public partial class MainMenuViewModel : ObservableObject
{
    [ObservableProperty]
    public partial IEnumerable<MainMenuItem> Items { get; set; }

    public MainMenuViewModel(IMenuService menuService) => Items = menuService.GetMainMenuItems();

    private bool _isAltGrPressed;

    public void RegisterHotKeys(MainWindow mainWindow)
    {
        mainWindow.AddHandler(InputElement.KeyDownEvent, (_, e) =>
        {
            if (e.PhysicalKey == PhysicalKey.AltRight)
            {
                _isAltGrPressed = true;
            }
        }, RoutingStrategies.Tunnel);

        mainWindow.AddHandler(InputElement.KeyUpEvent, (_, e) =>
        {
            if (e.PhysicalKey == PhysicalKey.AltRight)
            {
                _isAltGrPressed = false;
            }
        }, RoutingStrategies.Tunnel);
        
        mainWindow.AddHandler(InputElement.KeyDownEvent, (_, e) =>
        {
            if (e.Handled)
            {
                return;
            }

            foreach (MainMenuItem item in GetActiveMenuItemsWithHotkeys(Items))
            {
                if (string.IsNullOrEmpty(item.InputGestureText) || item.Command is null)
                {
                    continue;
                }

                KeyGesture gesture = KeyGestureUtilities.ParsePlatform(item.InputGestureText);

                if (_isAltGrPressed && gesture.KeyModifiers.HasFlag(KeyModifiers.Control) &&
                    gesture.KeyModifiers.HasFlag(KeyModifiers.Alt))
                {
                    continue;
                }

                if (!gesture.Matches(e))
                {
                    continue;
                }

                if (!item.Command.CanExecute(null))
                {
                    continue;
                }

                item.Command.Execute(null);
                e.Handled = true;
                return;
            }
        }, RoutingStrategies.Bubble);
    }

    private static IEnumerable<MainMenuItem> GetActiveMenuItemsWithHotkeys(IEnumerable<MainMenuItem> items)
    {
        foreach (MainMenuItem item in items)
        {
            if (item is { RegisterHotKey: true, IsVisible.Value: true, IsEnabled.Value: true } &&
                !string.IsNullOrEmpty(item.InputGestureText))
            {
                yield return item;
            }

            if (item.SubItems is null)
            {
                continue;
            }

            foreach (MainMenuItem subItem in GetActiveMenuItemsWithHotkeys(item.SubItems))
            {
                yield return subItem;
            }
        }
    }
}