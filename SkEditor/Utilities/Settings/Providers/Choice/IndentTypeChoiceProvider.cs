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

    public TranslatableString GetDisplayValue(object value)
    {
        return value switch
        {
            false => translationService.GetObservable(TranslationKey.SettingsGeneralTabTypeSpaces),
            true => translationService.GetObservable(TranslationKey.SettingsGeneralTabTypeTabs),
            _ => TranslatableString.FromRawString(value.ToString() ?? string.Empty)
        };
    }
}