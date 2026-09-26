using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Providers.Choice;
using AppSettingsProperty = SkEditor.Utilities.Settings.Configs.AppSettings.AppSettingsProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class AppearanceSectionProvider(
    AppSettings appSettings,
    ThemeChoiceProvider themeProvider,
    FontFamilyChoiceProvider fontFamilyProvider,
    IThemeService themes,
    ILinkService links) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        SettingSection appearance = SettingSection.Create("Appearance", TranslationKey.SettingsSectionAppearance)
            .WithOrder(20)
            .WithIcon(Symbol.PaintBrush);

        SettingSection appearanceTheme = CreateThemeSection().WithOrder(10);
        SettingSection appearanceFontDisplay = CreateFontDisplaySection().WithOrder(30);

        appearance.WithSubsections([appearanceTheme, appearanceFontDisplay]);

        return appearance;
    }

    private SettingSection CreateThemeSection()
    {
        return SettingSection.Create("Theme", TranslationKey.SettingsAppearanceSubsectionTheme)
            .WithIcon(Symbol.Color)
            .WithSettings(
            [
                ChoiceSetting<string>
                    .Create(TranslationKey.SettingsAppearanceThemeSelectedTheme)
                    .WithDescription(TranslationKey.SettingsAppearanceThemeSelectedThemeDescription)
                    .WithIcon(Symbol.PaintBrush)
                    .WithProperty(appSettings.GetProperty<string>(AppSettingsProperty.CurrentTheme))
                    .WithProvider(themeProvider),

                ActionSetting
                    .Create(TranslationKey.SettingsAppearanceThemeThemesFolder)
                    .WithDescription(TranslationKey.SettingsAppearanceThemeThemesFolderDescription)
                    .WithIcon(Symbol.FolderOpen)
                    .WithAction(TranslationKey.ButtonOpenFolder,
                        () => links.OpenFolder(themes.ThemeFolderPath)),

                ActionSetting
                    .Create(TranslationKey.SettingsAppearanceThemeReloadThemes)
                    .WithDescription(TranslationKey.SettingsAppearanceThemeReloadThemesDescription)
                    .WithIcon(Symbol.ArrowClockwise)
                    .WithAction(TranslationKey.ButtonReload, themes.ReloadThemesAsync)
            ]);
    }

    private SettingSection CreateFontDisplaySection()
    {
        return SettingSection
            .Create("Font Display", TranslationKey.SettingsAppearanceSubsectionFontDisplay)
            .WithIcon(Symbol.TextFont)
            .WithSettings(
            [
                ExpandableSetting
                    .Create(TranslationKey.SettingsAppearanceEditorFont)
                    .WithDescription(TranslationKey.SettingsAppearanceEditorFontDescription)
                    .WithIcon(Symbol.TextFont)
                    .WithChildren(
                        FontChoiceSetting
                            .Create(TranslationKey.SettingsAppearanceFontFamily)
                            .WithMinWidth(196)
                            .WithMaxWidth(196)
                            .WithProperty(appSettings.GetProperty<string?>(AppSettingsProperty.EditorFontFamily))
                            .WithProvider(fontFamilyProvider),
                        NumberSetting<int>
                            .Create(TranslationKey.SettingsAppearanceFontSize)
                            .WithProperty(appSettings.GetProperty<int>(AppSettingsProperty.EditorFontSize))
                            .WithMinValue(8)
                            .WithMaxValue(48)
                            .WithFormatString(),
                        ToggleSetting
                            .Create(TranslationKey.SettingsAppearanceFontLigatures)
                            .WithDescription(TranslationKey.SettingsAppearanceFontLigaturesDescription)
                            .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsFontLigaturesEnabled)),
                        ToggleSetting
                            .Create(TranslationKey.SettingsAppearanceMouseWheelZoom)
                            .WithDescription(TranslationKey.SettingsAppearanceMouseWheelZoomDescription)
                            .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsMouseWheelZoomEnabled)),
                        ToggleSetting
                            .Create(TranslationKey.SettingsEditorZoomSync)
                            .WithDescription(TranslationKey.SettingsEditorZoomSyncDescription)
                            .WithProperty(
                                appSettings.GetProperty<bool>(AppSettingsProperty.IsEditorZoomLevelSyncEnabled))
                    ),

                ToggleSetting
                    .Create(TranslationKey.SettingsAppearanceHighlightCurrentLine)
                    .WithIcon(Symbol.Highlight)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.HighlightCurrentLine))
            ]);
    }
}