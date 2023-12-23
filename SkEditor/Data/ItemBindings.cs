using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Views.Generators.Gui;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SkEditor.Data;
public partial class ItemBindings : ObservableObject, INotifyPropertyChanged
{
    [ObservableProperty]
    private ObservableCollection<Item> items = [];

    [ObservableProperty]
    private ObservableCollection<ComboBoxItem> filteredItems = [];
}
