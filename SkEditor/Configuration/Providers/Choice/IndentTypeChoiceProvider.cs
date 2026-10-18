using System.Collections.Generic;
using SkEditor.Core.Localization;
using SkEditor.Core.Localization;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Configuration.Providers.Choice;

public class IndentTypeChoiceProvider(ITranslationService translationService) : ISettingsChoiceProvider<bool>
{
    public IEnumerable<bool> GetValues()
    {
        return
        [
            true, // Tabs
            false // Spaces
        ];
    }

    public TranslatableString GetDisplayValue(bool value)
    {
        return value switch
        {
            false => translationService.GetObservable(TranslationKey.SettingsGeneralTabTypeSpaces),
            true => translationService.GetObservable(TranslationKey.SettingsGeneralTabTypeTabs)
        };
    }
}