using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Settings.Models;
using SkEditor.Utilities.Settings.Providers.Sections;

namespace SkEditor.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject
{
    public SettingsPageViewModel(IEnumerable<ISettingsSectionProvider> sectionProviders)
    {
        SettingsCategories = [];

        foreach (ISettingsSectionProvider settingsSectionProvider in sectionProviders)
        {
            SettingsCategories.Add(settingsSectionProvider.GetSection());
        }

        CurrentSection = SettingsCategories[0];
    }

    [ObservableProperty]
    public partial SettingSection? CurrentSection { get; set; }

    public List<SettingSection> SettingsCategories { get; }
}