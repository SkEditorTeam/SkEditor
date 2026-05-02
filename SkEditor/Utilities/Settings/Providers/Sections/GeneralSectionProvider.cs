using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Models;
using SkEditor.Utilities.Settings.Models.Types;
using SkEditor.Utilities.Settings.Providers.Choice;
using SkEditor.Utilities.Startup;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class GeneralSectionProvider(
    AppSettings appSettings,
    BinarySettings binarySettings,
    LanguageChoiceProvider languageProvider,
    StartupModeChoiceProvider startupModeProvider) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        return SettingSection.Create(TranslationKey.SettingsHomeGeneral)
            .WithIcon(Symbol.Home)
            .WithSettings(
            [
                ChoiceSetting<string>
                    .Create(TranslationKey.SettingsGeneralLanguage)
                    .WithIcon(Symbol.Earth)
                    .WithProperty(appSettings.GetProperty<string>(AppSettings.AppSettingsProperty.Language))
                    .WithProvider(languageProvider),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralCheckForUpdates)
                    .WithDescription(TranslationKey.SettingsGeneralCheckForUpdatesDescription)
                    .WithIcon(Symbol.CloudSync)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettings.AppSettingsProperty.CheckForAppUpdates)),

                ChoiceSetting<StartupMode>
                    .Create(TranslationKey.SettingsGeneralUseSplashScreen)
                    .WithDescription(TranslationKey.SettingsGeneralUseSplashScreenDescription)
                    .WithIcon(Symbol.Image)
                    .WithProperty(
                        binarySettings.GetProperty<StartupMode>(BinarySettings.BinarySettingsProperty.SplashScreenMode))
                    .WithProvider(startupModeProvider)
            ]);
    }
}