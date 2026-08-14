using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R3;
using SkEditor.Utilities.Settings;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.ViewModels.Controls;

public partial class BottomDrawerViewModel : ObservableObject
{
    public const double MinDrawerHeight = 120;
    
    [ObservableProperty]
    public partial double? DrawerHeight { get; set; } = null;

    [ObservableProperty]
    public partial bool IsDrawerOpen { get; private set; } = false;

    private double _lastHeight;

    public ObservableCollection<IDrawerPaneViewModel> Items { get; }

    public ObservableCollection<IDrawerPaneViewModel> VisibleItems { get; } = [];

    [ObservableProperty]
    public partial IDrawerPaneViewModel? SelectedItem { get; set; }

    private readonly BindableReactiveProperty<bool> _isBottomDrawerEnabled;

    public BottomDrawerViewModel(IEnumerable<IDrawerPaneViewModel> panes, ExperimentsConfig experiments,
        StateConfig state)
    {
        Items = new ObservableCollection<IDrawerPaneViewModel>(panes);

        foreach (IDrawerPaneViewModel pane in Items)
        {
            TrackVisibility(pane);
        }

        _lastHeight = state.BottomDrawerHeight;

        this.ObservePropertyChanged(x => x.DrawerHeight)
            .Subscribe(_ => state.BottomDrawerHeight = Math.Max(DrawerHeight ?? _lastHeight, MinDrawerHeight));

        _isBottomDrawerEnabled = VisibleItems
            .ObservePropertyChanged(x => x.Count)
            .Select(x => x > 0)
            .ToBindableReactiveProperty();

        _isBottomDrawerEnabled
            .Where(enabled => !enabled)
            .Subscribe(_ => CloseDrawer());
    }

    private void TrackVisibility(IDrawerPaneViewModel pane)
    {
        pane.IsVisible
            .Subscribe(isVisible => UpdateVisibility(pane, isVisible));
    }

    private void UpdateVisibility(IDrawerPaneViewModel pane, bool isVisible)
    {
        if (isVisible)
        {
            if (VisibleItems.Contains(pane))
            {
                return;
            }

            int sourceIndex = Items.IndexOf(pane);
            int insertAt = VisibleItems.Count;

            for (int i = 0; i < VisibleItems.Count; i++)
            {
                if (Items.IndexOf(VisibleItems[i]) <= sourceIndex)
                {
                    continue;
                }

                insertAt = i;
                break;
            }

            VisibleItems.Insert(insertAt, pane);
        }
        else
        {
            VisibleItems.Remove(pane);

            if (ReferenceEquals(SelectedItem, pane))
            {
                SelectedItem = VisibleItems.FirstOrDefault();
            }
        }
    }

    [RelayCommand]
    public void CloseDrawer()
    {
        IsDrawerOpen = false;
        _lastHeight = DrawerHeight ?? _lastHeight;
        DrawerHeight = null;
    }

    public void OpenDrawer()
    {
        if (!_isBottomDrawerEnabled.Value)
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

    public void TogglePane(string paneId)
    {
        string? oldSelectedId = SelectedItem?.Id;

        if (oldSelectedId == paneId)
        {
            ToggleDrawer();
        }
        else
        {
            SelectedItem = VisibleItems.FirstOrDefault(x => x.Id == paneId) ?? VisibleItems.FirstOrDefault();
            OpenDrawer();
        }
    }

    public T? GetPane<T>() where T : class, IDrawerPaneViewModel
    {
        foreach (IDrawerPaneViewModel item in Items)
        {
            if (item is T pane)
            {
                return pane;
            }
        }

        return null;
    }
}