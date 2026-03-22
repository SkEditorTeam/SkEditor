using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Tabs;

public abstract partial class Tab(TranslatableString header) : ObservableObject
{
    [ObservableProperty]
    private TranslatableString _header = header;

    public abstract IRelayCommand? CloseCommand { get; }

    protected void RequestClose() => WeakReferenceMessenger.Default.Send(new TabCloseRequestedMessage(this));
}

public record TabCloseRequestedMessage(Tab Tab);