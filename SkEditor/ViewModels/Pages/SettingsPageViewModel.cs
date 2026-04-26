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
    public SettingsPageViewModel(AppSettings appSettings, LanguageSettingsProvider languageSettingsProvider) =>
        SettingsCategories =
        [
            new SettingSection(TranslationKey.SettingsHomeGeneral, Symbol.Wrench, new List<SettingElement>
            {
                new ToggleSetting(TranslationKey.SettingsGeneralRpc, Symbol.BookOpenMicrophone,
                    TranslationKey.SettingsGeneralRpcDescription,
                    () => appSettings.IsDiscordRichPresenceEnabled,
                    value => appSettings.IsDiscordRichPresenceEnabled = value),

                new ChoiceSetting<string>(TranslationKey.SettingsGeneralLanguage, Symbol.Globe,
                    null,
                    languageSettingsProvider,
                    () => appSettings.Language,
                    value => appSettings.Language = value)
            })
        ];

    [ObservableProperty]
    public partial SettingSection? CurrentSection { get; set; }

    public List<SettingSection> SettingsCategories { get; }
}