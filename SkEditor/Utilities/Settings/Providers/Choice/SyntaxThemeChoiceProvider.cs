using System.Collections.Generic;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Models.UI;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class SyntaxThemeChoiceProvider(ISyntaxService syntax) : ISettingsChoiceProvider<SyntaxTheme?>
{
    public Observable<Unit> ValuesChanged => Observable.FromEvent(
        h => syntax.SyntaxesReloaded += h,
        h => syntax.SyntaxesReloaded -= h);
    
    public IEnumerable<SyntaxTheme?> GetValues()
    {
        return syntax.AvailableThemes;
    }

    public TranslatableString GetDisplayValue(SyntaxTheme? value)
    {
        return value?.Name?.ToTranslatable() ?? string.Empty.ToTranslatable();
    }
}