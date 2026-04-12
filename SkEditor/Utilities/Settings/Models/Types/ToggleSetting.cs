using System;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models.Types;

public partial class ToggleSetting : SettingElement
{
    private readonly Action<bool> _set;

    public ToggleSetting(TranslatableString name, FAIconSource icon, TranslatableString? desc, Func<bool> get,
        Action<bool> set)
        : base(name, icon, desc)
    {
        _set = set;
        IsEnabled = get();
    }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    partial void OnIsEnabledChanged(bool oldValue, bool newValue)
    {
        _set(newValue);
    }
}