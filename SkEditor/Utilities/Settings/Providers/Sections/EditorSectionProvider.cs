using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Models;
using SkEditor.Utilities.Settings.Models.Types;
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
        SettingSection editor = SettingSection.Create(TranslatableString.FromRawString("Editor"))
            .WithIcon(Symbol.Code);
        SettingSection editorBehavior = CreateBehaviorSection();
        SettingSection editorIndentation = CreateIndentationSection();

        editor.WithSubsections([editorBehavior, editorIndentation]);

        return editor;
    }

    private SettingSection CreateBehaviorSection()
    {
        return SettingSection.Create(TranslatableString.FromRawString("Behavior"))
            .WithIcon(Symbol.EditSettings)
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralWrapping)
                    .WithDescription(TranslationKey.SettingsGeneralWrappingDescription)
                    .WithIcon(Symbol.TextWrap)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsWordWrapEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralAutoSave)
                    .WithDescription(TranslationKey.SettingsGeneralAutoSaveDescription)
                    .WithIcon(Symbol.SaveSync)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsAutoSaveEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralChangeChecker)
                    .WithDescription(TranslationKey.SettingsGeneralChangeCheckerDescription)
                    .WithIcon(Symbol.DocumentSync)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.CheckForExternalFileChanges)),

                ToggleSetting
                    .Create(TranslatableString.FromRawString("Smooth scrolling"))
                    .WithDescription(TranslatableString.FromRawString("Enables smooth scrolling in the editor."))
                    .WithIcon(Symbol.Stream)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsEditorSmoothScrollEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralZoomSync)
                    .WithDescription(TranslationKey.SettingsGeneralZoomSyncDescription)
                    .WithIcon(Symbol.ZoomIn)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsEditorZoomLevelSyncEnabled))
            ]);
    }

    private SettingSection CreateIndentationSection()
    {
        return SettingSection
            .Create(TranslatableString.FromRawString("Indentation & Typing"))
            .WithIcon(Symbol.TextIndentIncrease)
            .WithSettings(
            [
                CompoundSetting.Create(TranslationKey.SettingsGeneralTabType)
                    .WithDescription(TranslationKey.SettingsGeneralTabTypeDescription)
                    .WithIcon(Symbol.TextIndentIncrease)
                    .WithChildren(
                        ChoiceSetting<bool>
                            .Create(TranslatableString.FromRawString("Indentation Type"))
                            .WithProperty(
                                appSettings.GetProperty<bool>(AppSettingsProperty.UseEditorTabsForIndentation))
                            .WithProvider(indentTypeProvider),
                        ChoiceSetting<int>
                            .Create(TranslatableString.FromRawString("Indentation Amount"))
                            .WithProperty(appSettings.GetProperty<int>(AppSettingsProperty.EditorTabSize))
                            .WithProvider(indentAmountProvider)
                    ),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralAutoIndent)
                    .WithDescription(TranslationKey.SettingsGeneralAutoIndentDescription)
                    .WithIcon(Symbol.ArrowAutofitWidth)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsAutoIndentEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralAutoPairing)
                    .WithDescription(TranslationKey.SettingsGeneralAutoPairingDescription)
                    .WithIcon(Symbol.TextQuote)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsAutoPairingEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralPasteIndent)
                    .WithDescription(TranslationKey.SettingsGeneralPasteIndentDescription)
                    .WithIcon(Symbol.ClipboardTextEdit)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsPasteIndentationEnabled))
            ]);
    }
}