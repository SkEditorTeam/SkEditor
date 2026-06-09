using FluentIcons.Common;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Providers.Choice;
using AppSettingsProperty = SkEditor.Utilities.Settings.Configs.AppSettings.AppSettingsProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class EditorSectionProvider(
    AppSettings appSettings,
    IndentTypeChoiceProvider indentTypeProvider,
    IndentAmountChoiceProvider indentAmountProvider) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        SettingSection editor = SettingSection.Create(TranslationKey.SettingsSectionEditor)
            .WithIcon(Symbol.Code);
        SettingSection editorBehavior = CreateBehaviorSection();
        SettingSection editorIndentation = CreateIndentationSection();

        editor.WithSubsections([editorBehavior, editorIndentation]);

        return editor;
    }

    private SettingSection CreateBehaviorSection()
    {
        return SettingSection.Create(TranslationKey.SettingsEditorSubsectionBehavior)
            .WithIcon(Symbol.EditSettings)
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsEditorWordWrap)
                    .WithDescription(TranslationKey.SettingsEditorWordWrapDescription)
                    .WithIcon(Symbol.TextWrap)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsWordWrapEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsEditorAutosave)
                    .WithDescription(TranslationKey.SettingsEditorAutosaveDescription)
                    .WithIcon(Symbol.SaveSync)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsAutoSaveEnabled)),

                // ToggleSetting
                //     .Create(TranslationKey.SettingsEditorExternalChangeChecker)
                //     .WithDescription(TranslationKey.SettingsEditorExternalChangeCheckerDescription)
                //     .WithIcon(Symbol.DocumentSync)
                //     .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.CheckForExternalFileChanges)),

                ToggleSetting
                    .Create(TranslationKey.SettingsEditorSmoothScrolling)
                    .WithDescription(TranslationKey.SettingsEditorSmoothScrollingDescription)
                    .WithIcon(Symbol.Stream)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsEditorSmoothScrollEnabled))
            ]);
    }

    private SettingSection CreateIndentationSection()
    {
        return SettingSection
            .Create(TranslationKey.SettingsEditorSubsectionIndentationTyping)
            .WithIcon(Symbol.TextIndentIncrease)
            .WithSettings(
            [
                CompoundSetting.Create(TranslationKey.SettingsEditorTabType)
                    .WithDescription(TranslationKey.SettingsEditorTabTypeDescription)
                    .WithIcon(Symbol.TextIndentIncrease)
                    .WithChildren(ChoiceSetting<bool>
                            .Create(TranslationKey.SettingsEditorIndentationType)
                            .WithProperty(
                                appSettings.GetProperty<bool>(AppSettingsProperty.UseEditorTabsForIndentation))
                            .WithProvider(indentTypeProvider), ChoiceSetting<int>
                            .Create(TranslationKey.SettingsEditorIndentationAmount)
                            .WithProperty(appSettings.GetProperty<int>(AppSettingsProperty.EditorTabSize))
                            .WithProvider(indentAmountProvider)
                    ),

                ToggleSetting
                    .Create(TranslationKey.SettingsEditorAutoIndent)
                    .WithDescription(TranslationKey.SettingsEditorAutoIndentDescription)
                    .WithIcon(Symbol.ArrowAutofitWidth)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsAutoIndentEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsEditorSmartAutoPairing)
                    .WithDescription(TranslationKey.SettingsEditorSmartAutoPairingDescription)
                    .WithIcon(Symbol.TextQuote)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsAutoPairingEnabled))

                // ToggleSetting
                //     .Create(TranslationKey.SettingsEditorPasteIndent)
                //     .WithDescription(TranslationKey.SettingsEditorPasteIndentDescription)
                //     .WithIcon(Symbol.ClipboardTextEdit)
                //     .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsPasteIndentationEnabled))
            ]);
    }
}