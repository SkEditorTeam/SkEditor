using System.Collections.Generic;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public interface ISettingsChoiceProvider<T>
{
    IEnumerable<T> GetValues();

    TranslatableString GetDisplayValue(T value)
    {
        return TranslatableString.FromRawString(value?.ToString() ?? string.Empty);
    }
}