using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Utilities.Parser.ViewModels;

public partial class ParserFilterViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private int _selectedFilterIndex = 0;
}