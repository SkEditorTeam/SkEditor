using System.Collections.Generic;
using System.IO;
using SkEditor.Contracts.Services;
using SkEditor.Models.UI;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class SyntaxGrammarChoiceProvider(ISyntaxService syntax) : ISettingsChoiceProvider<SyntaxGrammar?>
{
    public IEnumerable<SyntaxGrammar?> GetValues()
    {
        return syntax.AvailableGrammars;
    }

    public TranslatableString GetDisplayValue(SyntaxGrammar? value)
    {
        string? displayName = Path.GetFileNameWithoutExtension(value?.FileName)?.Replace(".tmLanguage", "");
        return displayName?.ToTranslatable() ?? string.Empty.ToTranslatable();
    }
}