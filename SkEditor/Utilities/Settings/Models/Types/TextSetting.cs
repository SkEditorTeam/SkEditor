using System;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models.Types;

public partial class TextSetting(TranslatableString name) : SettingElement<TextSetting>(name)
{
    public (Func<string> Get, Action<string> Set) Property
    {
        get => (field.Get, field.Set);
        set;
    }

    public TranslatableString? Placeholder { get; set; }

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
            Property.Set(value);
            Text = value;
        }
    }

    public static TextSetting Create(TranslatableString name)
    {
        return new TextSetting(name);
    }

    public TextSetting WithProperty((Func<string> Get, Action<string> Set) property)
    {
        Property = property;

        Value = property.Get();
        return this;
    }

    public TextSetting WithPassword(bool isPassword = true)
    {
        IsPassword = isPassword;
        return this;
    }

    public TextSetting WithWidth(double width)
    {
        Width = width;
        return this;
    }

    public TextSetting WithPlaceholder(TranslatableString placeholder)
    {
        Placeholder = placeholder;
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