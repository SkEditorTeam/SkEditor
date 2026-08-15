using System.Collections.Generic;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Views.Windows;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.ViewModels.Controls;

public partial class MainMenuViewModel : ObservableObject
{
    [ObservableProperty]
    public partial IEnumerable<MainMenuItem> Items { get; set; }

    public MainMenuViewModel(IMenuService menuService) => Items = menuService.GetMainMenuItems();

    public void RegisterHotKeys(MainWindow mainWindow)
    {
        RegisterHotKeysRecursive(Items, mainWindow);
    }

    private static void RegisterHotKeysRecursive(IEnumerable<MainMenuItem> items, MainWindow mainWindow)
    {
        foreach (MainMenuItem item in items)
        {
            if (!item.RegisterHotKey)
            {
                if (item.SubItems is not null)
                {
                    RegisterHotKeysRecursive(item.SubItems, mainWindow);
                }

                continue;
            }

            if (!string.IsNullOrEmpty(item.InputGestureText) && item.Command is not null)
            {
                string gestureText = item.InputGestureText;

                KeyBinding keyBinding = new()
                {
                    Command = item.Command,
                    Gesture = KeyGesture.Parse(gestureText)
                };

                item.IsVisible
                    .CombineLatest(item.IsEnabled, (visible, enabled) => visible && enabled)
                    .Subscribe(active =>
                    {
                        if (active)
                        {
                            if (!mainWindow.KeyBindings.Contains(keyBinding))
                            {
                                mainWindow.KeyBindings.Add(keyBinding);
                            }
                        }
                        else
                        {
                            mainWindow.KeyBindings.Remove(keyBinding);
                        }
                    });
            }

            if (item.SubItems is not null)
            {
                RegisterHotKeysRecursive(item.SubItems, mainWindow);
            }
        }
    }
}