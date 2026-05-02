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
        SettingSection appearance = SettingSection.Create(TranslationKey.SettingsHomeAppearance)
            .WithIcon(Symbol.PaintBrush);

        SettingSection appearanceTheme = CreateThemeSection();
        SettingSection appearanceFontDisplay = CreateFontDisplaySection();
        SettingSection appearanceSidebarLayout = CreateSidebarLayoutSection();

        appearance.WithSubsections([appearanceTheme, appearanceFontDisplay, appearanceSidebarLayout]);

        return appearance;
    }

    private SettingSection CreateThemeSection()
    {
        return SettingSection.Create(TranslatableString.FromRawString("Theme"))
            .WithIcon(Symbol.Color)
            .WithSettings(
            [
                ChoiceSetting<string>
                    .Create(TranslationKey.SettingsPersonalizationTheme)
                    .WithDescription(TranslationKey.SettingsPersonalizationThemeDescription)
                    .WithIcon(Symbol.PaintBrush)
                    .WithProperty(appSettings.GetProperty<string>(AppSettingsProperty.CurrentTheme))
                    .WithProvider(themeProvider),

                ActionSetting
                    .Create(TranslatableString.FromRawString("Themes Folder"))
                    .WithDescription(
                        TranslatableString.FromRawString("Open the Themes folder to create or add new themes."))
                    .WithIcon(Symbol.FolderOpen)
                    .WithAction(TranslationKey.SettingsThemeOpenFolder,
                        () => LinkOpener.OpenFolder(themes.ThemeFolderPath))
            ]);
    }

    private SettingSection CreateFontDisplaySection()
    {
        return SettingSection
            .Create(TranslatableString.FromRawString("Font & Display"))
            .WithIcon(Symbol.TextFont)
            .WithSettings(
            [
                ExpandableSetting
                    .Create(TranslatableString.FromRawString("Editor Font"))
                    .WithDescription(
                        TranslatableString.FromRawString("Change the font family and size used in the editor."))
                    .WithIcon(Symbol.TextFont)
                    .WithChildren(
                        ActionSetting
                            .Create(TranslatableString.FromRawString("Font Family"))
                            .WithDescription(translations.GetObservable(
                                TranslationKey.SettingsPersonalizationFontDescription,
                                appSettings.EditorFontFamily ?? "Default"))
                            .WithAction(TranslationKey.ButtonChange, async s =>
                            {
                                await SelectFont();
                                s.Description?.UpdateParameters(appSettings.EditorFontFamily ?? "Default");
                            }),
                        NumberSetting<int>
                            .Create(TranslatableString.FromRawString("Font Size"))
                            .WithProperty(appSettings.GetProperty<int>(AppSettingsProperty.EditorFontSize))
                            .WithMinValue(8)
                            .WithMaxValue(48)
                            .WithFormatString()
                    ),

                ToggleSetting
                    .Create(TranslationKey.SettingsPersonalizationHighlightCurrentLine)
                    .WithIcon(Symbol.Highlight)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.HighlightCurrentLine))
            ]);
    }

    private SettingSection CreateSidebarLayoutSection()
    {
        return SettingSection
            .Create(TranslatableString.FromRawString("Sidebar & Layout"))
            .WithIcon(Symbol.LayoutColumnTwo)
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsPersonalizationSidebarAnimations)
                    .WithDescription(TranslationKey.SettingsPersonalizationSidebarAnimationsDescription)
                    .WithIcon(Symbol.SlideTransition)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsSidebarAnimationEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralSidebarWidthSync)
                    .WithDescription(TranslationKey.SettingsGeneralSidebarWidthSyncDescription)
                    .WithIcon(Symbol.ColumnArrowRight)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsSidebarWidthSyncEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralProjectSingleFolderClick)
                    .WithDescription(TranslationKey.SettingsGeneralProjectSingleFolderClickDescription)
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