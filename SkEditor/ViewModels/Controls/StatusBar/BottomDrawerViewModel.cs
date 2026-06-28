using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SkEditor.ViewModels.Controls;

public partial class BottomDrawerViewModel : ObservableObject
{
    [ObservableProperty]
    public partial double? DrawerHeight { get; set; } = 200;

    [ObservableProperty]
    public partial bool IsDrawerOpen { get; private set; } = true;

    private double _lastHeight = 200;

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