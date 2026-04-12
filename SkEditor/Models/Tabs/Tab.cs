using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Tabs;

public abstract partial class Tab(TranslatableString header) : ObservableObject
{
    [ObservableProperty]
    public partial TranslatableString Header { get; set; } = header;

    [ObservableProperty]
    public partial bool IsSaved { get; set; } = false;

    public abstract IRelayCommand? CloseCommand { get; }

    public event EventHandler? CloseRequested;
    protected void RequestClose() => CloseRequested?.Invoke(this, EventArgs.Empty);
}