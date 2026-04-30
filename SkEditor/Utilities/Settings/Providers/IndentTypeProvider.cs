using System.Collections.Generic;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Providers;

public class IndentTypeProvider(ITranslationService translationService) : ISettingsProvider
{
    public IEnumerable<object> GetValues()
    {
        return
        [
            true, // Tabs
            false // Spaces
        ];
    }

    public string GetDisplayValue(object value)
    {
        return value switch
        {
            false => translationService.Get(TranslationKey.SettingsGeneralTabTypeSpaces),
            true => translationService.Get(TranslationKey.SettingsGeneralTabTypeTabs),
            _ => value.ToString() ?? string.Empty
        };
    }
}