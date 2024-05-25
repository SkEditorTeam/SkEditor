using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Docs;

namespace SkEditor.ViewModels;

public partial class DocumentationViewModel : ObservableObject
{

    [ObservableProperty] private SearchData _searchData = new();
    [ObservableProperty] private DocProvider? _provider;

}