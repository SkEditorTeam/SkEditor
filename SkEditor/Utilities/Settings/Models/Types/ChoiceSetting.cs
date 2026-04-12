using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
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

    public ChoiceSetting(TranslatableString name, FAIconSource icon, TranslatableString? desc,
        ISettingsProvider provider, Func<T> get, Action<T> set)
        : base(name, icon, desc)
    {
        _set = set;
        Options = provider.GetValues().Cast<T>().ToList();
        SelectedOption = get();
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