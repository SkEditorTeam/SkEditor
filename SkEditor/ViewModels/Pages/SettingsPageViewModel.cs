using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Settings.Models;
using AppSettings = SkEditor.Utilities.Settings.Configs.AppSettings;

namespace SkEditor.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject
{
    public SettingsPageViewModel(AppSettings appSettings)
    {
        SettingsCategories = appSettings.GetSettingsTree();

        if (SettingsCategories.Count > 0)
        {
            CurrentSection = SettingsCategories[0];
        }
    }

    [ObservableProperty]
    public partial SettingSection? CurrentSection { get; set; }

    public List<SettingSection> SettingsCategories { get; }
}