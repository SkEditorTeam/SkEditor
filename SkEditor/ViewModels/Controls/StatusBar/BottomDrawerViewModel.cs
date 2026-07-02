using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Utilities.Settings;

namespace SkEditor.ViewModels.Controls;

public partial class BottomDrawerViewModel : ObservableObject
{
    [ObservableProperty]
    public partial double? DrawerHeight { get; set; } = 0;

    [ObservableProperty]
    public partial bool IsDrawerOpen { get; private set; } = false;

    private double _lastHeight = 200;

    public ObservableCollection<IDrawerPaneViewModel> Items { get; }

    [ObservableProperty]
    public partial IDrawerPaneViewModel? SelectedItem { get; set; }

    private readonly ExperimentsConfig _experiments;

    public BottomDrawerViewModel(IEnumerable<IDrawerPaneViewModel> panes, ExperimentsConfig experiments)
    {
        Items = new ObservableCollection<IDrawerPaneViewModel>(panes);
        _experiments = experiments;

        _experiments.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(ExperimentsConfig.BottomDrawerEnabled))
            {
                return;
            }

            if (!_experiments.BottomDrawerEnabled)
            {
                CloseDrawer();
            }
        };

        if (!_experiments.BottomDrawerEnabled)
        {
            CloseDrawer();
        }
    }

    [RelayCommand]
    public void CloseDrawer()
    {
        IsDrawerOpen = false;
        _lastHeight = DrawerHeight ?? _lastHeight;
        DrawerHeight = 0;
    }

    public void OpenDrawer()
    {
        if (!_experiments.BottomDrawerEnabled)
        {
            return;
        }

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