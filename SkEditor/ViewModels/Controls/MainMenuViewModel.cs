using System.Collections.Generic;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Extensions;
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
            if (!item.RegisterHotKey) continue;

            if (!string.IsNullOrEmpty(item.InputGestureText) && item.Command is not null)
            {
                string gestureText = item.InputGestureText;
                string command = KeyModifiers.Command.ToString();
                if (gestureText.Contains("Ctrl") || gestureText.Contains("Control"))
                {
                    gestureText = gestureText.Replace("Ctrl", command).Replace("Control", command);
                }

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