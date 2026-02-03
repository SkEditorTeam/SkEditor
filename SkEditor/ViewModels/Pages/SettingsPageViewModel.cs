using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Localization;
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
        List<ToggleSetting> toggleSettings = appSettings.GetToggleSettingsSections();
        SettingsCategories =
        [
            new SettingSection(
                Translation.GetObservable("SettingsGeneralTitle"),
                new SymbolIconSource { Symbol = Symbol.Settings },
                toggleSettings
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

public abstract class SettingElement(string name, IconSource icon, string? description = null)
{
    public string Name { get; set; } = name;
    public IconSource Icon { get; set; } = icon;
    public string? Description { get; set; } = description;
}

[ObservableObject]
public partial class ToggleSetting : SettingElement
{
    private readonly Action<bool> _set;

    public ToggleSetting(string name, IconSource icon, string? desc, Func<bool> get, Action<bool> set)
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