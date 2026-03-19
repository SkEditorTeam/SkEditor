using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Models.Tabs;

public abstract partial class Tab(string header, object content) : ObservableObject
{
    [ObservableProperty]
    private string _header = header;

    [ObservableProperty]
    private object _content = content;
}