using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models.Types;

public partial class ToggleSetting : SettingElement
{
    private readonly Action<bool> _set;

    public ToggleSetting(TranslatableString name, SettingIcon icon, TranslatableString? desc,
        (Func<bool> Get, Action<bool> Set) property)
        : base(name, icon, desc)
    {
        _set = property.Set;
        IsEnabled = property.Get();
    }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    partial void OnIsEnabledChanged(bool oldValue, bool newValue)
    {
        _set(newValue);
    }
}