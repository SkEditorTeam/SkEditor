using SkEditor.Models.Settings;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public interface ISettingsSectionProvider
{
    string? ParentId => null;
    SettingSection GetSection();
}