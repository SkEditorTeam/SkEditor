using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Views.Generators.Gui;
using System.Collections.ObjectModel;

namespace SkEditor.Data;
public partial class ItemBindings : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Item> _items = [];

    [ObservableProperty]
    private ObservableCollection<ComboBoxItem> _filteredItems = [];
}
