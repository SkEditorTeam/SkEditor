using System.Collections.Generic;
using R3;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public interface ISettingsChoiceProvider<T>
{
    IEnumerable<T> GetValues();

    Observable<Unit>? ValuesChanged => null;

    TranslatableString GetDisplayValue(T value)
    {
        return TranslatableString.FromRawString(value?.ToString() ?? string.Empty);
    }

    string? GetFontFamily(T value)
    {
        return null;
    }
}