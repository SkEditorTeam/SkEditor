using System.Collections.Generic;
using System.IO;
using System.Linq;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Models.UI;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class SyntaxGrammarChoiceProvider(ISyntaxService syntax) : ISettingsChoiceProvider<SyntaxGrammar?>
{
    public Observable<Unit> ValuesChanged => Observable.FromEvent(
        h => syntax.SyntaxesReloaded += h,
        h => syntax.SyntaxesReloaded -= h);
    
    public IEnumerable<SyntaxGrammar?> GetValues()
    {
        List<SyntaxGrammar?> grammars = [null, ..syntax.AvailableGrammars.Where(g => g.ScopeName != "source.sk")];

        return grammars;
    }

    public TranslatableString GetDisplayValue(SyntaxGrammar? value)
    {
        if (value is null)
        {
            return "Auto".ToTranslatable();
        }

        string? displayName = Path.GetFileNameWithoutExtension(value.FileName)?.Replace(".tmLanguage", "");
        return displayName?.ToTranslatable() ?? string.Empty.ToTranslatable();
    }
}

public class SyntaxGrammarSkriptChoiceProvider(ISyntaxService syntax) : ISettingsChoiceProvider<SyntaxGrammar?>
{
    public Observable<Unit> ValuesChanged => Observable.FromEvent(
        h => syntax.SyntaxesReloaded += h,
        h => syntax.SyntaxesReloaded -= h);

    public IEnumerable<SyntaxGrammar?> GetValues()
    {
        return syntax.AvailableGrammars.Where(g => g.ScopeName == "source.sk");
    }

    public TranslatableString GetDisplayValue(SyntaxGrammar? value)
    {
        string? displayName = Path.GetFileNameWithoutExtension(value?.FileName)?.Replace(".tmLanguage", "");
        return displayName?.ToTranslatable() ?? string.Empty.ToTranslatable();
    }
}