using System.Collections.Generic;
using System.Linq;
using SkEditor.Contracts.Services;

namespace SkEditor.Utilities.Settings.Providers;

public class ThemeSettingsProvider(IThemeService themeService) : ISettingsProvider
{
    public IEnumerable<object> GetValues()
    {
        List<string> themes = themeService.Themes.Select(x => x.Name).ToList();
        return themes.Distinct();
    }
}