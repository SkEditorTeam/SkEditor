using System.Collections.Generic;
using System.IO;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class SyntaxThemeChoiceProvider : ISettingsChoiceProvider<string>
{
    public IEnumerable<string> GetValues()
    {
        return ["dark_skript", "ethereal_skript"];
    }

    public TranslatableString GetDisplayValue(string value)
    {
        return value switch
        {
            "dark_skript" => "Default dark".ToTranslatable(),
            "ethereal_skript" => "Ethereal".ToTranslatable(),
            _ => throw new InvalidDataException($"Invalid syntax theme value: {value}")
        };
    }
}