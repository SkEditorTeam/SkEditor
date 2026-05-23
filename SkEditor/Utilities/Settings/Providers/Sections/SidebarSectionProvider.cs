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
        return SettingSection.Create(TranslationKey.SettingsSectionSidebar)
            .WithIcon(Symbol.PanelLeft)
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsSidebarSingleFolderClick)
                    .WithDescription(TranslationKey.SettingsSidebarSingleFolderClickDescription)
                    .WithIcon(Symbol.CursorClick)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsProjectSingleClickEnabled))
            ]);
    }
}