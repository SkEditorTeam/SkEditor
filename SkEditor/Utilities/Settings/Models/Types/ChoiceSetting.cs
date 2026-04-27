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

public partial class ChoiceSetting<T> : SettingElement, IChoiceSetting
{
    private readonly Action<T> _set;

    public ChoiceSetting(TranslatableString name, SettingIcon icon, TranslatableString? desc,
        ISettingsProvider provider, (Func<T> Get, Action<T> Set) property)
        : base(name, icon, desc)
    {
        _set = property.Set;

        Options = provider.GetValues().Cast<T>().ToList();
        SelectedOption = property.Get();
    }

    [ObservableProperty]
    public partial T SelectedOption { get; set; }

    public List<T> Options { get; }

    IEnumerable IChoiceSetting.Options => Options;

    object? IChoiceSetting.SelectedItem
    {
        get => SelectedOption;
        set => SelectedOption = (T)value!;
    }

    partial void OnSelectedOptionChanged(T? oldValue, T newValue)
    {
        _set(newValue);
    }
}