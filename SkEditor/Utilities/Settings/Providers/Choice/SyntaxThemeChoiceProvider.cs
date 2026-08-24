using System.Collections.Generic;
using SkEditor.Contracts.Services;
using SkEditor.Models.UI;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class SyntaxThemeChoiceProvider(ISyntaxService syntax) : ISettingsChoiceProvider<SyntaxTheme?>
{
    public IEnumerable<SyntaxTheme?> GetValues()
    {
        return syntax.AvailableThemes;
    }

    public TranslatableString GetDisplayValue(SyntaxTheme? value)
    {
        return value?.Name?.ToTranslatable() ?? string.Empty.ToTranslatable();
    }
}