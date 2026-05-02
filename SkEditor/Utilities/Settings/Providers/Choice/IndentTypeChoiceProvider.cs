using System.Collections.Generic;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class IndentTypeChoiceProvider(ITranslationService translationService) : ISettingsChoiceProvider
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