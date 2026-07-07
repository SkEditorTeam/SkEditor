using FluentIcons.Common;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Providers.Choice;
using AppSettingsProperty = SkEditor.Utilities.Settings.Configs.AppSettings.AppSettingsProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class GeneralSectionProvider(
    AppSettings appSettings,
    LanguageChoiceProvider languageProvider) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        return SettingSection.Create("General", TranslationKey.SettingsSectionGeneral)
            .WithIcon(Symbol.Home)
            .WithSettings(
            [
                ChoiceSetting<string>
                    .Create(TranslationKey.SettingsGeneralLanguage)
                    .WithIcon(Symbol.Earth)
                    .WithProperty(appSettings.GetProperty<string>(AppSettingsProperty.Language))
                    .WithProvider(languageProvider),

                // ToggleSetting
                //     .Create(TranslationKey.SettingsGeneralCheckForUpdates)
                //     .WithDescription(TranslationKey.SettingsGeneralCheckForUpdatesDescription)
                //     .WithIcon(Symbol.CloudSync)
                //     .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.CheckForAppUpdates))
            ]);
    }
}