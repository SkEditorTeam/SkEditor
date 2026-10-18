using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;
using SkEditor.Editor.Syntax;
using SkEditor.Editor.Syntax.Models;

namespace SkEditor.Configuration.Providers.Choice;

public class SyntaxThemeChoiceProvider(ISyntaxService syntax) : ISettingsChoiceProvider<SyntaxTheme?>
{
    public Observable<Unit> ValuesChanged => Observable.FromEvent(
        h => syntax.SyntaxesReloaded += h,
        h => syntax.SyntaxesReloaded -= h);

    public IEnumerable<SyntaxTheme?> GetValues()
    {
        return syntax.AvailableThemes.Where(t =>
            t.TargetLanguage?.Equals("skript", StringComparison.OrdinalIgnoreCase) != true);
    }

    public TranslatableString GetDisplayValue(SyntaxTheme? value)
    {
        return value?.Name?.ToTranslatable() ?? string.Empty.ToTranslatable();
    }
}

public class SyntaxThemeSkriptChoiceProvider(ISyntaxService syntax) : ISettingsChoiceProvider<SyntaxTheme?>
{
    public Observable<Unit> ValuesChanged => Observable.FromEvent(
        h => syntax.SyntaxesReloaded += h,
        h => syntax.SyntaxesReloaded -= h);

    public IEnumerable<SyntaxTheme?> GetValues()
    {
        return syntax.AvailableThemes.Where(t =>
            t.TargetLanguage?.Equals("skript", StringComparison.OrdinalIgnoreCase) == true &&
            !t.IsBuiltInTextMateTheme);
    }

    public TranslatableString GetDisplayValue(SyntaxTheme? value)
    {
        return value?.Name?.ToTranslatable() ?? string.Empty.ToTranslatable();
    }
}