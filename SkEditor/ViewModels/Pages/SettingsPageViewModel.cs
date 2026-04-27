using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Models;
using SkEditor.Utilities.Settings.Models.Types;
using SkEditor.Utilities.Settings.Providers;
using AppSettings = SkEditor.Utilities.Settings.Configs.AppSettings;

namespace SkEditor.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject
{
    public SettingsPageViewModel(AppSettings appSettings, LanguageSettingsProvider languageSettingsProvider)
    {
        SettingsCategories =
        [
            new SettingSection(TranslationKey.SettingsHomeGeneral, Symbol.Wrench, new List<SettingElement>
            {
                new ChoiceSetting<string>(TranslationKey.SettingsGeneralLanguage, Symbol.Earth,
                    null,
                    languageSettingsProvider,
                    appSettings.GetProperty<string>(nameof(appSettings.Language))),

                new ToggleSetting(TranslationKey.SettingsGeneralCheckForUpdates, Symbol.CloudSync,
                    TranslationKey.SettingsGeneralCheckForUpdatesDescription,
                    appSettings.GetProperty<bool>(nameof(appSettings.CheckForAppUpdates))),

                new ToggleSetting(TranslationKey.SettingsGeneralRpc, "DiscordIcon",
                    TranslationKey.SettingsGeneralRpcDescription,
                    appSettings.GetProperty<bool>(nameof(appSettings.IsDiscordRichPresenceEnabled)))
            })
        ];

        CurrentSection = SettingsCategories[0];
    }

    [ObservableProperty]
    public partial SettingSection? CurrentSection { get; set; }

    public List<SettingSection> SettingsCategories { get; }
}