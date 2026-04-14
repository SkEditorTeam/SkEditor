using System.Collections.Generic;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Contracts.Services;
using SkEditor.Models.Menu;
using SkEditor.Views.Windows;

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
            if (!item.RegisterHotKey) continue;
            
            if (!string.IsNullOrEmpty(item.InputGestureText) && item.Command is not null)
            {
                KeyBinding keyBinding = new()
                {
                    Command = item.Command,
                    Gesture = KeyGesture.Parse(item.InputGestureText)
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