using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Models;
using AppSettings = SkEditor.Utilities.Settings.Configs.AppSettings;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private SettingSection _currentSection;

    public SettingsPageViewModel(AppSettings appSettings)
    {
        List<SettingElement> settings = appSettings.GetToggleSettingsSections();
        SettingsCategories =
        [
            new SettingSection(
                Translation.GetObservable(TranslationKey.SettingsGeneralTitle),
                new SymbolIconSource { Symbol = Symbol.Settings },
                settings
            )
        ];

        _currentSection = SettingsCategories[0];
    }

    public List<SettingSection> SettingsCategories { get; }
}