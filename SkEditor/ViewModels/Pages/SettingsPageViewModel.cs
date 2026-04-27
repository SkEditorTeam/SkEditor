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
        SettingsCategories = [];

        SettingSection general = SettingSection.Create(TranslationKey.SettingsHomeGeneral)
            .WithIcon(Symbol.Wrench)
            .WithSettings(
            [
                ChoiceSetting<string>
                    .Create(TranslationKey.SettingsGeneralLanguage)
                    .WithIcon(Symbol.Earth)
                    .WithProperty(appSettings.GetProperty<string>(nameof(appSettings.Language)))
                    .WithProvider(languageSettingsProvider),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralCheckForUpdates)
                    .WithDescription(TranslationKey.SettingsGeneralCheckForUpdatesDescription)
                    .WithIcon(Symbol.CloudSync)
                    .WithProperty(appSettings.GetProperty<bool>(nameof(appSettings.CheckForAppUpdates))),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralRpc)
                    .WithDescription(TranslationKey.SettingsGeneralRpcDescription)
                    .WithIcon("DiscordIcon")
                    .WithProperty(appSettings.GetProperty<bool>(nameof(appSettings.IsDiscordRichPresenceEnabled)))
            ]);

        SettingsCategories.Add(general);

        CurrentSection = SettingsCategories[0];
    }

    [ObservableProperty]
    public partial SettingSection? CurrentSection { get; set; }

    public List<SettingSection> SettingsCategories { get; }
}