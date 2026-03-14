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
    private IEnumerable<MainMenuItem> _items = new List<MainMenuItem>();

    public MainMenuViewModel(IMenuService menuService) => Items = menuService.GetMainMenuItems();

    public void RegisterHotKeys(MainWindow mainWindow)
    {
        RegisterHotKeysRecursive(Items);
        return;

        void RegisterHotKeysRecursive(IEnumerable<MainMenuItem> items)
        {
            foreach (MainMenuItem item in items)
            {
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
                    RegisterHotKeysRecursive(item.SubItems);
                }
            }
        }
    }
}