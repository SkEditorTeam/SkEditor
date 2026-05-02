using System.Collections.Generic;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public interface ISettingsChoiceProvider
{
    IEnumerable<object> GetValues();

    string GetDisplayValue(object value)
    {
        return value?.ToString() ?? string.Empty;
    }
}