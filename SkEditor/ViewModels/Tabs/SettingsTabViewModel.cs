using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Models.Settings;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Providers.Sections;

namespace SkEditor.ViewModels.Tabs;

public partial class SettingsTabViewModel : TabViewModelBase
{
    private readonly IConfigService _configs;

    public SettingsTabViewModel(IConfigService configs, IEnumerable<ISettingsSectionProvider> sectionProviders) : base(
        TranslationKey.SettingsHomeTitle, Symbol.Settings)
    {
        _configs = configs;
        SettingsCategories = [];

        foreach (ISettingsSectionProvider settingsSectionProvider in sectionProviders)
        {
            SettingsCategories.Add(settingsSectionProvider.GetSection());
        }

        CurrentSection = SettingsCategories[0];
    }

    public override IRelayCommand CloseCommand => CloseTabCommand;

    public override bool IsCacheable => true;

    [ObservableProperty]
    public partial SettingSection? CurrentSection { get; set; }

    public List<SettingSection> SettingsCategories { get; }

    [RelayCommand]
    private void CloseTab()
    {
        _ = _configs.SaveAllConfigsAsync();
        RequestClose();
    }
}