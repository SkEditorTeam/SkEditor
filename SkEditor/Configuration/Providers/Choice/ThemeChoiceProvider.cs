using System.Collections.Generic;
using System.Linq;
using R3;
using SkEditor.Contracts.Services;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class ThemeChoiceProvider(IThemeService themeService) : ISettingsChoiceProvider<string>
{
    public Observable<Unit> ValuesChanged => Observable.FromEvent(
        h => themeService.ThemesReloaded += h,
        h => themeService.ThemesReloaded -= h);
    
    public IEnumerable<string> GetValues()
    {
        List<string> themes = themeService.Themes.Select(x => x.Name).ToList();
        return themes.Distinct();
    }
}