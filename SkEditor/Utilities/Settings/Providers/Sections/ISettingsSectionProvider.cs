using SkEditor.Utilities.Settings.Models;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public interface ISettingsSectionProvider
{
    SettingSection GetSection();
}