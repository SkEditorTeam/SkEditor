using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Docs;

namespace SkEditor.ViewModels;

public partial class DocumentationViewModel : ObservableObject
{
    [ObservableProperty] private DocProvider? _provider;

    [ObservableProperty] private SearchData _searchData = new();
}