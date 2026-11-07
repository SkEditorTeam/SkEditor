using FluentIcons.Common;
using SkEditor.Configuration.Configs;
using SkEditor.Configuration.Models;
using SkEditor.Configuration.Models.Types;
using SkEditor.Configuration.Providers.Choice;
using SkEditor.Core.Localization;
using AppSettingsProperty = SkEditor.Configuration.Configs.AppSettings.AppSettingsProperty;

namespace SkEditor.Configuration.Providers.Sections;

public class EditorSectionProvider(
    AppSettings appSettings,
    IndentTypeChoiceProvider indentTypeProvider,
    IndentAmountChoiceProvider indentAmountProvider) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        SettingSection editor = SettingSection.Create("Editor", TranslationKey.SettingsSectionEditor)
            .WithOrder(10)
            .WithIcon(Symbol.Code);
        SettingSection editorBehavior = CreateBehaviorSection();
        SettingSection editorIndentation = CreateIndentationSection();

        editor.WithSubsections([editorBehavior, editorIndentation]);

        return editor;
    }

    private SettingSection CreateBehaviorSection()
    {
        return SettingSection.Create("Editor Behavior", TranslationKey.SettingsEditorSubsectionBehavior)
            .WithIcon(Symbol.EditSettings)
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsEditorWordWrap)
                    .WithDescription(TranslationKey.SettingsEditorWordWrapDescription)
                    .WithIcon(Symbol.TextWrap)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsWordWrapEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsEditorSmoothScrolling)
                    .WithDescription(TranslationKey.SettingsEditorSmoothScrollingDescription)
                    .WithIcon(Symbol.Stream)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsEditorSmoothScrollEnabled)),
                
                ExpandableSetting
                    .Create(TranslationKey.SettingsEditorColorPreview)
                    .WithDescription(TranslationKey.SettingsEditorColorPreviewDescription)
                    .WithIcon(Symbol.Color)
                    .WithChildren(
                        ToggleSetting
                            .Create(TranslationKey.SettingsEditorColorPreviewOption)
                            .WithDescription(TranslationKey.SettingsEditorColorPreviewOptionDescription)
                            .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.ColorPreviewEnabled)),
                        ToggleSetting
                            .Create(TranslationKey.SettingsEditorColorPreviewGlobal)
                            .WithDescription(TranslationKey.SettingsEditorColorPreviewGlobalDescription)
                            .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.GlobalColorPreviewEnabled))
                    ),
            ]);
    }

    private SettingSection CreateIndentationSection()
    {
        return SettingSection
            .Create("Indentation and Typing", TranslationKey.SettingsEditorSubsectionIndentationTyping)
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