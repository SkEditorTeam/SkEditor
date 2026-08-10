using System;
using R3;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Settings.Types;

public class ToggleSetting : SettingElement<ToggleSetting>
{
    public ToggleSetting(BindableReactiveProperty<TranslatableString> name) : base(name)
    {
        IsToggled = new BindableReactiveProperty<bool>(false);

        IsToggled.Subscribe(newValue =>
        {
            if (Property.Get() != newValue)
            {
                Property.Set(newValue);
            }
        });
    }

    public (Func<bool> Get, Action<bool> Set) Property
    {
        get => (field.Get, field.Set);
        set
        {
            field = value;
            IsToggled.Value = value.Get();
        }
    }

    public BindableReactiveProperty<bool> IsToggled { get; set; }

    public static ToggleSetting Create(TranslatableString name)
    {
        return new ToggleSetting(new BindableReactiveProperty<TranslatableString>(name));
    }

    public static ToggleSetting Create(BindableReactiveProperty<TranslatableString> name)
    {
        return new ToggleSetting(name);
    }

    public ToggleSetting WithProperty((Func<bool> Get, Action<bool> Set) property)
    {
        Property = property;
        return this;
    }
}