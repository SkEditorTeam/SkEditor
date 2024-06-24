using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Controls.Sidebar;

public partial class ParserFilterViewModel : ObservableObject
{
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private int _selectedFilterIndex = 0;
}