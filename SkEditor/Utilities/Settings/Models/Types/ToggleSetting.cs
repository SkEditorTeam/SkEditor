using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models.Types;

public partial class ToggleSetting(TranslatableString name) : SettingElement(name)
{
    public (Func<bool> Get, Action<bool> Set) Property
    {
        get => (field.Get, field.Set);
        set
        {
            field = value;
            IsEnabled = value.Get();
        }
    }

    [ObservableProperty]
    public partial bool IsEnabled { get; set; }

    public static ToggleSetting Create(TranslatableString name)
    {
        return new ToggleSetting(name);
    }

    public ToggleSetting WithIcon(SettingIcon icon)
    {
        Icon = icon;
        return this;
    }

    public ToggleSetting WithDescription(TranslatableString description)
    {
        Description = description;
        return this;
    }

    public ToggleSetting WithProperty((Func<bool> Get, Action<bool> Set) property)
    {
        Property = property;
        return this;
    }

    partial void OnIsEnabledChanged(bool oldValue, bool newValue)
    {
        if (oldValue != newValue)
        {
            Property.Set(newValue);
        }
    }
}