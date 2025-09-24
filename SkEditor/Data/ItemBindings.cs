using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Views.Windows.Generators.Gui;

namespace SkEditor.Data;

public partial class ItemBindings : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ComboBoxItem> _filteredItems = [];

    [ObservableProperty] private ObservableCollection<Item> _items = [];
}