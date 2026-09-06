using System;
using System.Collections.ObjectModel;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Providers.Choice;
using SkEditor.Utilities.Startup;
using AppSettingsProperty = SkEditor.Utilities.Settings.Configs.AppSettings.AppSettingsProperty;
using BinarySettingsProperty = SkEditor.Utilities.Settings.Configs.BinarySettings.BinarySettingsProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class AdvancedSectionProvider(
    IConfigService configs,
    AppSettings appSettings,
    BinarySettings binarySettings,
    StartupModeChoiceProvider startupModeProvider,
    ILinkService links) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        ObservableCollection<SettingElement> settings =
        [
            ToggleSetting
                .Create(TranslationKey.SettingsAdvancedDeveloperMode)
                .WithDescription(TranslationKey.SettingsAdvancedDeveloperModeDescription)
                .WithIcon(Symbol.Flash)
                .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsDevModeEnabled)),

            ChoiceSetting<StartupMode>
                .Create(TranslationKey.SettingsAdvancedStartupSplashScreen)
                .WithDescription(TranslationKey.SettingsAdvancedStartupSplashScreenDescription)
                .WithIcon(Symbol.Image)
                .WithProperty(
                    binarySettings.GetProperty<StartupMode>(BinarySettingsProperty.SplashScreenMode))
                .WithProvider(startupModeProvider)
        ];

        if (OperatingSystem.IsMacOS())
        {
            settings.Add(
                ToggleSetting
                    .Create(TranslationKey.SettingsAdvancedUseMacOsIntegratedMenu)
                    .WithDescription(TranslationKey.SettingsAdvancedUseMacOsIntegratedMenuDescription)
                    .WithIcon(Symbol.PanelTopGallery)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.UseIntegratedMenuOnMacOs))
            );
        }

        settings.Add(
            ActionSetting
                .Create(TranslationKey.SettingsAdvancedOpenAppDataFolder)
                .WithDescription(TranslationKey.SettingsAdvancedOpenAppDataFolderDescription)
                .WithIcon(Symbol.FolderOpen)
                .WithAction(TranslationKey.ButtonOpenFolder,
                    () => links.OpenFolder(configs.GetAppDataPath()))
        );

        return SettingSection.Create("Advanced", TranslationKey.SettingsSectionAdvanced)
            .WithIcon(Symbol.Wrench)
            .WithSettings(settings);
    }
}