using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.ViewModels.Controls;

public partial class SidebarViewModel : ObservableObject
{
    public ISidebarService Sidebar { get; }
    
    private readonly StateConfig _stateConfig;
    private double _savedColumnWidth;

    [ObservableProperty]
    public partial double? ColumnWidth { get; set; } = null;

    public bool IsPanelVisible => Sidebar.SelectedPanel is not null;
    public ISidebarPanelViewModel? LastSelectedItem { get; private set; }

    public SidebarViewModel(ISidebarService sidebar, StateConfig stateConfig)
    {
        Sidebar = sidebar;
        _stateConfig = stateConfig;
        _savedColumnWidth = _stateConfig.SidebarPaneSize;

        Sidebar.PropertyChanged += OnSidebarPropertyChanged;
    }

    private void OnSidebarPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ISidebarService.SelectedPanel))
        {
            return;
        }

        if (Sidebar.SelectedPanel is not null)
        {
            LastSelectedItem = Sidebar.SelectedPanel;
            OnPropertyChanged(nameof(LastSelectedItem));
        }

        OnPropertyChanged(nameof(IsPanelVisible));

        if (Sidebar.SelectedPanel == null)
        {
            if (ColumnWidth.HasValue)
            {
                _savedColumnWidth = ColumnWidth.Value;
            }

            ColumnWidth = null;
        }
        else
        {
            ColumnWidth ??= _savedColumnWidth;
        }
    }

    partial void OnColumnWidthChanged(double? value)
    {
        if (value.HasValue)
        {
            _stateConfig.SidebarPaneSize = value.Value;
        }
    }
}