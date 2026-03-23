using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Tabs;

public abstract partial class Tab(TranslatableString header) : ObservableObject
{
    [ObservableProperty]
    private TranslatableString _header = header;

    public abstract IRelayCommand? CloseCommand { get; }

    public event EventHandler? CloseRequested;
    protected void RequestClose() => CloseRequested?.Invoke(this, EventArgs.Empty);
}