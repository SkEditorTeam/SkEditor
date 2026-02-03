using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Providers;
using AppSettings = SkEditor.Utilities.Settings.Configs.AppSettings;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private SettingSection _currentSection;

    public List<SettingSection> SettingsCategories { get; }

    public SettingsPageViewModel()
    {
        AppSettings appSettings = SkEditorAPI.Configs.GetAppSettings();
        List<SettingElement> settings = appSettings.GetToggleSettingsSections();
        SettingsCategories =
        [
            new SettingSection(
                Translation.GetObservable("SettingsGeneralTitle"),
                new SymbolIconSource { Symbol = Symbol.Settings },
                settings
            )
        ];

        _currentSection = SettingsCategories[0];
    }
}

public class SettingSection(
    TranslatableString name,
    IconSource icon,
    IEnumerable<SettingElement> settings,
    List<SettingSection>? subsections = null) : ObservableObject
{
    public TranslatableString Name { get; set; } = name;
    public IconSource Icon { get; set; } = icon;
    public List<SettingElement> Settings { get; set; } = settings.ToList();
    public List<SettingSection> Subsections { get; set; } = subsections ?? [];
}

public abstract class SettingElement(TranslatableString name, IconSource icon, TranslatableString? description = null)
    : ObservableObject
{
    public TranslatableString Name { get; set; } = name;
    public IconSource Icon { get; set; } = icon;
    public TranslatableString? Description { get; set; } = description;
}

public partial class ToggleSetting : SettingElement
{
    private readonly Action<bool> _set;

    public ToggleSetting(TranslatableString name, IconSource icon, TranslatableString? desc, Func<bool> get, Action<bool> set)
        : base(name, icon, desc)
    {
        _set = set;
        IsEnabled = get();
    }

    [ObservableProperty]
    private bool _isEnabled;

    partial void OnIsEnabledChanged(bool oldValue, bool newValue)
    {
        _set(newValue);
    }
}

public interface IChoiceSetting
{
    IEnumerable Options { get; }
    object? SelectedItem { get; set; }
}

public partial class ChoiceSetting<T> : SettingElement, IChoiceSetting
{
    private readonly Action<T> _set;
    public List<T> Options { get; }

    IEnumerable IChoiceSetting.Options => Options;

    object? IChoiceSetting.SelectedItem
    {
        get => SelectedOption;
        set => SelectedOption = (T)value!;
    }

    public ChoiceSetting(TranslatableString name, IconSource icon, TranslatableString? desc,
        ISettingsProvider provider, Func<T> get, Action<T> set)
        : base(name, icon, desc)
    {
        _set = set;
        Options = provider.GetValues().Cast<T>().ToList();
        SelectedOption = get();
    }

    [ObservableProperty]
    private T _selectedOption;

    partial void OnSelectedOptionChanged(T? oldValue, T newValue)
    {
        _set(newValue);
    }
}