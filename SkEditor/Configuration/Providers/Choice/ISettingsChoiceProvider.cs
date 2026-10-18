using System.Collections.Generic;
using R3;
using SkEditor.Core.Localization;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Configuration.Providers.Choice;

public interface ISettingsChoiceProvider<T>
{
    IEnumerable<T> GetValues();

    Observable<Unit>? ValuesChanged => null;

    TranslatableString GetDisplayValue(T value)
    {
        return TranslatableString.FromRawString(value?.ToString() ?? string.Empty);
    }
}