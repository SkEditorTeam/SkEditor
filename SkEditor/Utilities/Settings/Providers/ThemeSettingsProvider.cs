using System.Collections.Generic;
using System.Linq;
using SkEditor.Services;

namespace SkEditor.Utilities.Settings.Providers;

public class ThemeSettingsProvider : ISettingsProvider
{
    public IEnumerable<object> GetValues()
    {
        List<string> themes = ThemeService.ServiceStatic.Themes.Select(x => x.Name).ToList();
        return themes.Distinct();
    }
}