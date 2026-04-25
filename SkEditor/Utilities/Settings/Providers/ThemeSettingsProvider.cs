using System;
using System.Collections.Generic;

namespace SkEditor.Utilities.Settings.Providers;

public class ThemeSettingsProvider : ISettingsProvider
{
    public IEnumerable<object> GetValues()
    {
        //List<string> themes = ThemeService.ServiceStatic.Themes.Select(x => x.Name).ToList();
        //return themes.Distinct();
        throw new NotImplementedException();
    }
}