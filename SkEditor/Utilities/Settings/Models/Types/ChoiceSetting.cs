using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Providers;

namespace SkEditor.Utilities.Settings.Models.Types;

public interface IChoiceSetting
{
    IEnumerable Options { get; }
    object? SelectedItem { get; set; }
}

public partial class ChoiceSetting<T>(TranslatableString name) : SettingElement(name), IChoiceSetting
{
    public (Func<T> Get, Action<T> Set) Property
    {
        get => (field.Get, field.Set);
        set
        {
            field = value;
            SelectedOption = value.Get();
        }
    }

    public ISettingsProvider? Provider { get; protected set; }

    [ObservableProperty]
    public partial T SelectedOption { get; set; }

    public List<T> Options { get; protected set; } = [];

    IEnumerable IChoiceSetting.Options => Options;

    object? IChoiceSetting.SelectedItem
    {
        get => SelectedOption;
        set => SelectedOption = (T)value!;
    }

    public static ChoiceSetting<T> Create(TranslatableString name)
    {
        return new ChoiceSetting<T>(name);
    }

    public ChoiceSetting<T> WithIcon(SettingIcon icon)
    {
        Icon = icon;
        return this;
    }

    public ChoiceSetting<T> WithDescription(TranslatableString description)
    {
        Description = description;
        return this;
    }

    public ChoiceSetting<T> WithProperty((Func<T> Get, Action<T> Set) property)
    {
        Property = property;
        return this;
    }

    public ChoiceSetting<T> WithProvider(ISettingsProvider provider)
    {
        Provider = provider;
        Options = provider.GetValues().Cast<T>().ToList();
        SelectedOption = Property.Get();

        return this;
    }

    partial void OnSelectedOptionChanged(T? oldValue, T newValue)
    {
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            Property.Set(newValue);
        }
    }
}