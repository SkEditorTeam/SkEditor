using System.Collections.Generic;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Contracts.Services;
using SkEditor.Views.Windows;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.ViewModels.Controls;

public partial class MainMenuViewModel : ObservableObject
{
    public MainMenuViewModel(IMenuService menuService) => Items = menuService.GetMainMenuItems();

    [ObservableProperty]
    public partial IEnumerable<MainMenuItem> Items { get; set; }

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
                mainWindow.KeyBindings.Add(keyBinding);
            }

            if (item.SubItems is not null)
            {
                RegisterHotKeysRecursive(item.SubItems, mainWindow);
            }
        }
    }
}