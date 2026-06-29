using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SkEditor.ViewModels.Controls;

public partial class BottomDrawerViewModel(IEnumerable<IDrawerPaneViewModel> panes) : ObservableObject
{
    [ObservableProperty]
    public partial double? DrawerHeight { get; set; } = 0;

    [ObservableProperty]
    public partial bool IsDrawerOpen { get; private set; } = false;

    private double _lastHeight = 200;

    public ObservableCollection<IDrawerPaneViewModel> Items { get; } = new(panes);

    [ObservableProperty]
    public partial IDrawerPaneViewModel? SelectedItem { get; set; }

    [RelayCommand]
    public void CloseDrawer()
    {
        IsDrawerOpen = false;
        _lastHeight = DrawerHeight ?? _lastHeight;
        DrawerHeight = 0;
    }

    public void OpenDrawer()
    {
        IsDrawerOpen = true;
        DrawerHeight = _lastHeight;
    }

    public void ToggleDrawer()
    {
        if (IsDrawerOpen)
        {
            CloseDrawer();
        }
        else
        {
            OpenDrawer();
        }
    }
}