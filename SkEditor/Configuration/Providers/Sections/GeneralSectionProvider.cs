using System;
using FluentIcons.Common;
using SkEditor.Configuration.Configs;
using SkEditor.Configuration.Models;
using SkEditor.Configuration.Models.Types;
using SkEditor.Configuration.Providers.Choice;
using SkEditor.Core.Localization;
using AppSettingsProperty = SkEditor.Configuration.Configs.AppSettings.AppSettingsProperty;

namespace SkEditor.Configuration.Providers.Sections;

public class GeneralSectionProvider(
    AppSettings appSettings,
    LanguageChoiceProvider languageProvider) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        SettingSection section = SettingSection.Create("General", TranslationKey.SettingsSectionGeneral)
            .WithIcon(Symbol.Home)
            .WithOrder(0)
            .WithSettings(
            [
                ChoiceSetting<string>
                    .Create(TranslationKey.SettingsGeneralLanguage)
                    .WithIcon(Symbol.Earth)
                    .WithProperty(appSettings.GetProperty<string>(AppSettingsProperty.Language))
                    .WithProvider(languageProvider),
                
                ExpandableSetting
                    .Create(TranslationKey.SettingsGeneralUpdates)
                    .WithDescription(TranslationKey.SettingsGeneralUpdatesDescription)
                    .WithIcon(Symbol.ArrowSync)
                    .WithChildren(
                        ToggleSetting
                            .Create(TranslationKey.SettingsGeneralCheckForUpdates)
                            .WithDescription(TranslationKey.SettingsGeneralCheckForUpdatesDescription)
                            .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.CheckForUpdates)),
                        ToggleSetting
                            .Create(TranslationKey.SettingsGeneralIncludePreReleases)
                            .WithDescription(TranslationKey.SettingsGeneralIncludePreReleasesDescription)
                            .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IncludePreReleaseUpdates))
                    ),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralSessionRestore)
                    .WithDescription(TranslationKey.SettingsGeneralSessionRestoreDescription)
                    .WithIcon(Symbol.History)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsSessionRestoreEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsEditorAutosave)
                    .WithDescription(TranslationKey.SettingsEditorAutosaveDescription)
                    .WithIcon(Symbol.SaveSync)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsAutoSaveEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralExternalChangeDetect)
                    .WithDescription(TranslationKey.SettingsGeneralExternalChangeDetectDescription)
                    .WithIcon(Symbol.DocumentSync)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsChangeDetectEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsExplorerSingleClickOpen)
                    .WithDescription(TranslationKey.SettingsExplorerSingleClickOpenDescription)
                    .WithIcon(Symbol.CursorClick)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsExplorerSingleClickOpenEnabled))
            ]);

        if (OperatingSystem.IsWindows())
        {
            ToggleSetting forwardSlashesCopy = ToggleSetting
                .Create(TranslationKey.SettingsExplorerForwardSlashesCopy)
                .WithDescription(TranslationKey.SettingsExplorerForwardSlashesCopyDescription)
                .WithIcon(Symbol.ClipboardLink)
                .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsExplorerForwardSlashesCopyEnabled));

            section.AddSetting(forwardSlashesCopy);
        }

        return section;
    }
}