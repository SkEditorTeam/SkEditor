using FluentIcons.Common;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using AppSettingsProperty = SkEditor.Utilities.Settings.Configs.AppSettings.AppSettingsProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class SidebarSectionProvider(AppSettings appSettings) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        return SettingSection.Create(TranslatableString.FromRawString("Sidebar"))
            .WithIcon(Symbol.PanelLeft)
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsAppearanceProjectSingleFolderClick)
                    .WithDescription(TranslationKey.SettingsAppearanceProjectSingleFolderClickDescription)
                    .WithIcon(Symbol.CursorClick)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsProjectSingleClickEnabled))
            ]);
    }
}