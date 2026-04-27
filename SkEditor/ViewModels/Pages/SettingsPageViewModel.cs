using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Models;
using SkEditor.Utilities.Settings.Models.Types;
using SkEditor.Utilities.Settings.Providers;
using SkEditor.Utilities.Startup;
using AppSettings = SkEditor.Utilities.Settings.Configs.AppSettings;

namespace SkEditor.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject
{
    public SettingsPageViewModel(AppSettings appSettings, BinarySettings binarySettings,
        LanguageSettingsProvider languageSettingsProvider,
        ThemeSettingsProvider themeSettingsProvider,
        StartupModeSettingsProvider startupModeSettingsProvider)
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
                    .WithProperty(appSettings.GetProperty<bool>(nameof(appSettings.IsDiscordRichPresenceEnabled))),

                ChoiceSetting<StartupMode>
                    .Create(TranslationKey.SettingsGeneralUseSplashScreen)
                    .WithDescription(TranslationKey.SettingsGeneralUseSplashScreenDescription)
                    .WithIcon(Symbol.Image)
                    .WithProperty(binarySettings.GetProperty<StartupMode>(nameof(binarySettings.SplashScreenMode)))
                    .WithProvider(startupModeSettingsProvider)
            ]);

        SettingSection appearrance = SettingSection.Create(TranslationKey.SettingsHomeAppearance)
            .WithIcon(Symbol.PaintBrush)
            .WithSettings(
            [
                ChoiceSetting<string>
                    .Create(TranslationKey.SettingsPersonalizationTheme)
                    .WithDescription(TranslationKey.SettingsPersonalizationThemeDescription)
                    .WithIcon(Symbol.PaintBrush)
                    .WithProperty(appSettings.GetProperty<string>(nameof(appSettings.CurrentTheme)))
                    .WithProvider(themeSettingsProvider),

                ToggleSetting
                    .Create(TranslationKey.SettingsPersonalizationSidebarAnimations)
                    .WithDescription(TranslationKey.SettingsPersonalizationSidebarAnimationsDescription)
                    .WithIcon(Symbol.SlideTransition)
                    .WithProperty(appSettings.GetProperty<bool>(nameof(appSettings.IsSidebarAnimationEnabled)))
            ]);

        SettingsCategories.AddRange([general, appearrance]);

        CurrentSection = SettingsCategories[0];
    }

    [ObservableProperty]
    public partial SettingSection? CurrentSection { get; set; }

    public List<SettingSection> SettingsCategories { get; }
}