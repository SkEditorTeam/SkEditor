using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Utilities.Docs;

public partial class SearchData : ObservableObject
{
    [ObservableProperty] private string _filteredAddon = "";
    [ObservableProperty] private IDocumentationEntry.Type _filteredType = IDocumentationEntry.Type.All;

    [ObservableProperty] private string _query = "";
}