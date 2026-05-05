using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.ViewModels.Controls;

public partial class SidebarViewModel : ObservableObject
{
    private double _savedColumnWidth = 250.0;

    public SidebarViewModel(IEnumerable<ISidebarPanelViewModel> panels)
    {
        Items = new ObservableCollection<ISidebarPanelViewModel>(panels);

        PropertyChanged += OnPropertyChanged;
    }

    public ObservableCollection<ISidebarPanelViewModel> Items { get; }

    [ObservableProperty]
    public partial ISidebarPanelViewModel? SelectedItem { get; set; }

    [ObservableProperty]
    public partial double? ColumnWidth { get; set; } = null;

    private void OnPropertyChanged(object? _, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(SelectedItem))
        {
            return;
        }

        if (SelectedItem == null)
        {
            if (ColumnWidth.HasValue)
            {
                _savedColumnWidth = ColumnWidth.Value;
            }

            ColumnWidth = null;
        }
        else if (ColumnWidth == null)
        {
            ColumnWidth = _savedColumnWidth;
        }
    }
}