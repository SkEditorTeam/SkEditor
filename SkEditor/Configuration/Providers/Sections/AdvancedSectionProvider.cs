using System;
using System.Collections.ObjectModel;
using FluentIcons.Common;
using SkEditor.Configuration.Configs;
using SkEditor.Configuration.Models;
using SkEditor.Configuration.Models.Types;
using SkEditor.Configuration.Providers.Choice;
using SkEditor.Core.System;
using SkEditor.Core.Localization;
using SkEditor.Windowing.Models;
using AppSettingsProperty = SkEditor.Configuration.Configs.AppSettings.AppSettingsProperty;
using BinarySettingsProperty = SkEditor.Configuration.Configs.BinarySettings.BinarySettingsProperty;

namespace SkEditor.Configuration.Providers.Sections;

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

            ToggleSetting
                .Create(TranslationKey.SettingsAdvancedHardwareAcceleration)
                .WithDescription(TranslationKey.SettingsAdvancedHardwareAccelerationDescription)
                .WithIcon(Symbol.DeveloperBoardLightning)
                .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.UseHardwareAcceleration)),

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

        settings.Add(
            ActionSetting
                .Create(TranslationKey.SettingsAdvancedOpenLocalAppDataFolder)
                .WithDescription(TranslationKey.SettingsAdvancedOpenLocalAppDataFolderDescription)
                .WithIcon(Symbol.FolderOpen)
                .WithAction(TranslationKey.ButtonOpenFolder,
                    () => links.OpenFolder(configs.GetLocalAppDataPath()))
        );

        return SettingSection.Create("Advanced", TranslationKey.SettingsSectionAdvanced)
            .WithIcon(Symbol.Wrench)
            .WithOrder(int.MaxValue)
            .WithSettings(settings);
    }
}