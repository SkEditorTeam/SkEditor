using SkEditor.Configuration.Models;

namespace SkEditor.Configuration.Providers.Sections;

public interface ISettingsSectionProvider
{
    string? ParentId => null;
    SettingSection GetSection();
}