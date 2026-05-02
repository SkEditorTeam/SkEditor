using System.Collections.Generic;
using System.Linq;
using SkEditor.Contracts.Services;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class ThemeChoiceProvider(IThemeService themeService) : ISettingsChoiceProvider
{
    public IEnumerable<object> GetValues()
    {
        List<string> themes = themeService.Themes.Select(x => x.Name).ToList();
        return themes.Distinct();
    }
}