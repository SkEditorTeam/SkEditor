using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;

namespace SkEditor.ViewModels.Controls;

public partial class BottomDrawerViewModel(INotificationService notifs) : ObservableObject
{
    [ObservableProperty]
    public partial double? DrawerHeight { get; set; } = 0;

    [ObservableProperty]
    public partial bool IsDrawerOpen { get; private set; } = false;

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

    [RelayCommand]
    public void ErrorClick()
    {
        notifs.Debug("you clicked the error");
    }
}