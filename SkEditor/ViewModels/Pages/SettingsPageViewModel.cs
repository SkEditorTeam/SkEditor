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
            .WithIcon(Symbol.Home)
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

                ChoiceSetting<StartupMode>
                    .Create(TranslationKey.SettingsGeneralUseSplashScreen)
                    .WithDescription(TranslationKey.SettingsGeneralUseSplashScreenDescription)
                    .WithIcon(Symbol.Image)
                    .WithProperty(binarySettings.GetProperty<StartupMode>(nameof(binarySettings.SplashScreenMode)))
                    .WithProvider(startupModeSettingsProvider)
            ]);

        SettingSection editor = SettingSection.Create(TranslatableString.FromRawString("Editor"))
            .WithIcon(Symbol.Code);

        SettingSection editorBehavior = SettingSection.Create(TranslatableString.FromRawString("Behavior"))
            .WithIcon(Symbol.EditSettings)
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralWrapping)
                    .WithDescription(TranslationKey.SettingsGeneralWrappingDescription)
                    .WithIcon(Symbol.TextWrap)
                    .WithProperty(appSettings.GetProperty<bool>(nameof(appSettings.IsWordWrapEnabled))),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralAutoSave)
                    .WithDescription(TranslationKey.SettingsGeneralAutoSaveDescription)
                    .WithIcon(Symbol.SaveSync)
                    .WithProperty(appSettings.GetProperty<bool>(nameof(appSettings.IsAutoSaveEnabled))),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralChangeChecker)
                    .WithDescription(TranslationKey.SettingsGeneralChangeCheckerDescription)
                    .WithIcon(Symbol.DocumentSync)
                    .WithProperty(appSettings.GetProperty<bool>(nameof(appSettings.CheckForExternalFileChanges))),

                ToggleSetting
                    .Create(TranslatableString.FromRawString("Smooth scrolling"))
                    .WithDescription(TranslatableString.FromRawString("Enables smooth scrolling in the editor."))
                    .WithIcon(Symbol.ArrowCircleDown)
                    .WithProperty(appSettings.GetProperty<bool>(nameof(appSettings.IsEditorSmoothScrollEnabled))),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralZoomSync)
                    .WithDescription(TranslationKey.SettingsGeneralZoomSyncDescription)
                    .WithIcon(Symbol.ZoomIn)
                    .WithProperty(appSettings.GetProperty<bool>(nameof(appSettings.IsEditorZoomLevelSyncEnabled)))
            ]);

        SettingSection editorIndentation = SettingSection
            .Create(TranslatableString.FromRawString("Indentation & Typing"))
            .WithIcon(Symbol.TextIndentIncrease)
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralAutoIndent)
                    .WithDescription(TranslationKey.SettingsGeneralAutoIndentDescription)
                    .WithIcon(Symbol.ArrowAutofitWidth)
                    .WithProperty(appSettings.GetProperty<bool>(nameof(appSettings.IsAutoIndentEnabled))),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralAutoPairing)
                    .WithDescription(TranslationKey.SettingsGeneralAutoPairingDescription)
                    .WithIcon(Symbol.TextQuote)
                    .WithProperty(appSettings.GetProperty<bool>(nameof(appSettings.IsAutoPairingEnabled))),
                
                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralPasteIndent)
                    .WithDescription(TranslationKey.SettingsGeneralPasteIndentDescription)
                    .WithIcon(Symbol.ClipboardTextEdit)
                    .WithProperty(appSettings.GetProperty<bool>(nameof(appSettings.IsPasteIndentationEnabled))),
            ]);

        editor.WithSubsections([editorBehavior, editorIndentation]);

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

        SettingsCategories.AddRange([general, editor, appearrance]);

        CurrentSection = SettingsCategories[0];
    }

    [ObservableProperty]
    public partial SettingSection? CurrentSection { get; set; }

    public List<SettingSection> SettingsCategories { get; }
}