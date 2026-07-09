using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Settings.Types;

public partial class ToggleSetting(TranslatableString name) : SettingElement<ToggleSetting>(name)
{
    public (Func<bool> Get, Action<bool> Set) Property
    {
        get => (field.Get, field.Set);
        set
        {
            field = value;
            IsToggled = value.Get();
        }
    }

    [ObservableProperty]
    public partial bool IsToggled { get; set; }

    public static ToggleSetting Create(TranslatableString name)
    {
        return new ToggleSetting(name);
    }

    public ToggleSetting WithProperty((Func<bool> Get, Action<bool> Set) property)
    {
        Property = property;
        return this;
    }

    partial void OnIsToggledChanged(bool oldValue, bool newValue)
    {
        if (oldValue != newValue)
        {
            Property.Set(newValue);
        }
    }
}