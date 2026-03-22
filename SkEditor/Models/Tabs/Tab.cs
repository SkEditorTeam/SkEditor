using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace SkEditor.Models.Tabs;

public abstract partial class Tab(string header, object content) : ObservableObject
{
    [ObservableProperty]
    private string _header = header;

    [ObservableProperty]
    private object _content = content;
    
    public abstract IRelayCommand? CloseCommand { get; }
    
    protected void RequestClose() => WeakReferenceMessenger.Default.Send(new TabCloseRequestedMessage(this));
}

public record TabCloseRequestedMessage(Tab Tab);