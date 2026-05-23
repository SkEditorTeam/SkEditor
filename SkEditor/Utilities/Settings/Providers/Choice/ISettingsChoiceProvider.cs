using System.Collections.Generic;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public interface ISettingsChoiceProvider
{
    IEnumerable<object> GetValues();

    TranslatableString GetDisplayValue(object value)
    {
        return TranslatableString.FromRawString(value.ToString() ?? string.Empty);
    }
}