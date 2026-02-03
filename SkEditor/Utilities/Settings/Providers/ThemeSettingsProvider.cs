using System.Collections.Generic;
using System.Linq;
using SkEditor.Utilities.Styling;

namespace SkEditor.Utilities.Settings.Providers;

public class ThemeSettingsProvider : ISettingsProvider
{
    public IEnumerable<object> GetValues()
    {
        List<string> themes = ThemeEditor.Themes.Select(x => x.Name).ToList();
        return themes.Distinct();
    }
}