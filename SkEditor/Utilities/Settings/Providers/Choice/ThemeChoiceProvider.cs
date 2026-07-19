using System.Collections.Generic;
using System.Linq;
using SkEditor.Contracts.Services;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class ThemeChoiceProvider(IThemeService themeService) : ISettingsChoiceProvider<string>
{
    public IEnumerable<string> GetValues()
    {
        List<string> themes = themeService.Themes.Select(x => x.Name).ToList();
        return themes.Distinct();
    }
}