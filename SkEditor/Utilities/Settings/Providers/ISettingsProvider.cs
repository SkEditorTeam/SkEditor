using System.Collections.Generic;

namespace SkEditor.Utilities.Settings.Providers;

public interface ISettingsProvider
{
    IEnumerable<object> GetValues();
    
    string GetDisplayValue(object value) => value?.ToString() ?? string.Empty;
}