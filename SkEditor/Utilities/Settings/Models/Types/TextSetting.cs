using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models.Types;

public partial class TextSetting(TranslatableString name) : SettingElement(name)
{
    public (Func<string> Get, Action<string> Set) Property
    {
        get => (field.Get, field.Set);
        set;
    }

    public bool IsPassword { get; set; }
    public char PasswordChar => IsPassword ? '●' : '\0';

    public override double Width { get; set; }

    [ObservableProperty]
    public partial string Text { get; set; }

    public string Value
    {
        get => Property.Get();
        set
        {
            if (Property.Get() == value)
            {
                return;
            }

            Property.Set(value);
            Text = value;
        }
    }

    public static TextSetting Create(TranslatableString name)
    {
        return new TextSetting(name);
    }

    public TextSetting WithIcon(SettingIcon icon)
    {
        Icon = icon;
        return this;
    }

    public TextSetting WithDescription(TranslatableString description)
    {
        Description = description;
        return this;
    }

    public TextSetting WithProperty((Func<string> Get, Action<string> Set) property)
    {
        Property = property;

        Value = property.Get();
        return this;
    }

    partial void OnTextChanged(string oldValue, string newValue)
    {
        if (oldValue == newValue)
        {
            return;
        }

        Property.Set(newValue);
    }
}