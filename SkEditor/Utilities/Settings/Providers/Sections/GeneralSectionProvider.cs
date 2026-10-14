using System;
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