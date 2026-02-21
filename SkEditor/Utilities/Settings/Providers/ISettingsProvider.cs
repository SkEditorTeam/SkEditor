using System.Collections.Generic;

namespace SkEditor.Utilities.Settings.Providers;

public interface ISettingsProvider
{
    IEnumerable<object> GetValues();

    string GetDisplayValue(object value)
    {
        return value?.ToString() ?? string.Empty;
    }
}