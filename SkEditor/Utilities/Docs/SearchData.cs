using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Utilities.Docs;

public partial class SearchData : ObservableObject
{
    
    [ObservableProperty] private string _query;
    [ObservableProperty] private List<IDocumentationEntry.Type> _filteredTypes;
    [ObservableProperty] private ObservableCollection<string> _filteredAddons;

}