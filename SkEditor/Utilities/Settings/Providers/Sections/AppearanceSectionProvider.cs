using System.Threading.Tasks;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Providers.Choice;
using SkEditor.Views.Windows;
using AppSettingsProperty = SkEditor.Utilities.Settings.Configs.AppSettings.AppSettingsProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class AppearanceSectionProvider(
    AppSettings appSettings,
    ThemeChoiceProvider themeProvider,
    IThemeService themes,
    ITranslationService translations,
    WindowRegistry windowRegistry,
    ILinkService links) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        SettingSection appearance = SettingSection.Create(TranslationKey.SettingsSectionAppearance)
            .WithIcon(Symbol.PaintBrush);

        SettingSection appearanceTheme = CreateThemeSection();
        SettingSection appearanceFontDisplay = CreateFontDisplaySection();
        SettingSection appearanceSidebarLayout = CreateSidebarLayoutSection();

        appearance.WithSubsections([appearanceTheme, appearanceFontDisplay, appearanceSidebarLayout]);

        return appearance;
    }

    private SettingSection CreateThemeSection()
    {
        return SettingSection.Create(TranslationKey.SettingsAppearanceSubsectionTheme)
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
                        () => links.OpenFolder(themes.ThemeFolderPath))
            ]);
    }

    private SettingSection CreateFontDisplaySection()
    {
        return SettingSection
            .Create(TranslationKey.SettingsAppearanceSubsectionFontDisplay)
            .WithIcon(Symbol.TextFont)
            .WithSettings(
            [
                ExpandableSetting
                    .Create(TranslationKey.SettingsAppearanceEditorFont)
                    .WithDescription(TranslationKey.SettingsAppearanceEditorFontDescription)
                    .WithIcon(Symbol.TextFont)
                    .WithChildren(
                        ActionSetting
                            .Create(TranslationKey.SettingsAppearanceFontFamily)
                            .WithDescription(translations.GetObservable(
                                TranslationKey.SettingsAppearanceCurrentFontDescription,
                                GetFontName()))
                            .WithAction(TranslationKey.ButtonChange, async s =>
                            {
                                await SelectFont();
                                s.Description?.UpdateParameters(GetFontName());
                            }), NumberSetting<int>
                            .Create(TranslationKey.SettingsAppearanceFontSize)
                            .WithProperty(appSettings.GetProperty<int>(AppSettingsProperty.EditorFontSize))
                            .WithMinValue(8)
                            .WithMaxValue(48)
                            .WithFormatString(),
                        ToggleSetting
                            .Create(TranslationKey.SettingsAppearanceMouseWheelZoom)
                            .WithDescription(TranslationKey.SettingsAppearanceMouseWheelZoomDescription)
                            .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsMouseWheelZoomEnabled))
                    ),

                ToggleSetting
                    .Create(TranslationKey.SettingsAppearanceHighlightCurrentLine)
                    .WithIcon(Symbol.Highlight)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.HighlightCurrentLine))
            ]);
    }

    private SettingSection CreateSidebarLayoutSection()
    {
        return SettingSection
            .Create(TranslationKey.SettingsAppearanceSubsectionSidebarLayout)
            .WithIcon(Symbol.PanelLeft)
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsExplorerSingleClickOpen)
                    .WithDescription(TranslationKey.SettingsExplorerSingleClickOpenDescription)
                    .WithIcon(Symbol.CursorClick)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsExplorerSingleClickOpenEnabled))
            ]);
    }

    private async Task SelectFont()
    {
        if (windowRegistry.Active is not { } mainWindow)
        {
            return;
        }

        FontSelectionWindow window = new();
        string result = await window.ShowDialog<string>(mainWindow);
        if (result is null)
        {
            return;
        }

        appSettings.EditorFontFamily = result == "Default"
            ? null
            : result;
    }

    private object GetFontName()
    {
        if (appSettings.EditorFontFamily is not null)
        {
            return appSettings.EditorFontFamily;
        }

        return (TranslatableString)TranslationKey.SettingsAppearanceCurrentFontDescriptionDefault;
    }
}