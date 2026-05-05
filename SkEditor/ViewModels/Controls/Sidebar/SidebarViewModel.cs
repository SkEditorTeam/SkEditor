using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.ViewModels.Controls;

public partial class SidebarViewModel : ObservableObject
{
    private readonly StateConfig _stateConfig;
    private double _savedColumnWidth;

    public SidebarViewModel(IEnumerable<ISidebarPanelViewModel> panels, StateConfig stateConfig)
    {
        Items = new ObservableCollection<ISidebarPanelViewModel>(panels);
        _stateConfig = stateConfig;

        _savedColumnWidth = _stateConfig.SidebarPaneSize;

        PropertyChanged += OnPropertyChanged;
    }

    public ObservableCollection<ISidebarPanelViewModel> Items { get; }

    [ObservableProperty]
    public partial ISidebarPanelViewModel? SelectedItem { get; set; }

    [ObservableProperty]
    public partial double? ColumnWidth { get; set; } = null;

    private void OnPropertyChanged(object? _, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedItem) when SelectedItem == null:
            {
                if (ColumnWidth.HasValue)
                {
                    _savedColumnWidth = ColumnWidth.Value;
                }

                ColumnWidth = null;
                break;
            }
            case nameof(SelectedItem):
            {
                ColumnWidth ??= _savedColumnWidth;

                break;
            }
            case nameof(ColumnWidth):
            {
                if (ColumnWidth.HasValue)
                {
                    _stateConfig.SidebarPaneSize = ColumnWidth.Value;
                }

                break;
            }
        }
    }
}