using System.Threading.Tasks;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Models;
using SkEditor.Utilities.Settings.Models.Types;
using SkEditor.Utilities.Settings.Providers.Choice;
using SkEditor.Views.Windows;
using AppSettingsProperty = SkEditor.Utilities.Settings.Configs.AppSettings.AppSettingsProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class AppearanceSectionProvider(
    AppSettings appSettings,
    ThemeChoiceProvider themeProvider,
    IThemeService themes,
    ITranslationService translations,
    WindowRegistry windowRegistry) : ISettingsSectionProvider
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
                    .WithAction(TranslationKey.SettingsAppearanceThemeOpenFolderButton,
                        () => LinkOpener.OpenFolder(themes.ThemeFolderPath))
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
                                appSettings.EditorFontFamily ?? "Default"))
                            .WithAction(TranslationKey.ButtonChange, async s =>
                            {
                                await SelectFont();
                                s.Description?.UpdateParameters(appSettings.EditorFontFamily ?? "Default");
                            }),
                        NumberSetting<int>
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
            .WithIcon(Symbol.LayoutColumnTwo)
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsAppearanceSidebarAnimations)
                    .WithDescription(TranslationKey.SettingsAppearanceSidebarAnimationsDescription)
                    .WithIcon(Symbol.SlideTransition)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsSidebarAnimationEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsAppearanceSyncSidebarWidth)
                    .WithDescription(TranslationKey.SettingsAppearanceSyncSidebarWidthDescription)
                    .WithIcon(Symbol.ColumnArrowRight)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsSidebarWidthSyncEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsAppearanceProjectSingleFolderClick)
                    .WithDescription(TranslationKey.SettingsAppearanceProjectSingleFolderClickDescription)
                    .WithIcon(Symbol.FolderArrowRight)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsProjectSingleClickEnabled))
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
}