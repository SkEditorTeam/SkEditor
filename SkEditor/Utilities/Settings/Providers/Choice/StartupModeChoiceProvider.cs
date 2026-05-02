using System.Collections.Generic;
using System.IO;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Startup;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class StartupModeChoiceProvider(ITranslationService translationService) : ISettingsChoiceProvider
{
    public IEnumerable<object> GetValues()
    {
        return [StartupMode.Auto, StartupMode.DisableSplashScreen, StartupMode.EnableSplashScreen];
    }

    public string GetDisplayValue(object value)
    {
        return value switch
        {
            StartupMode.Auto => translationService.Get(TranslationKey.SettingsGeneralUseSplashScreenAuto),
            StartupMode.DisableSplashScreen => translationService.Get(TranslationKey
                .SettingsGeneralUseSplashScreenDisabled),
            StartupMode.EnableSplashScreen => translationService.Get(TranslationKey
                .SettingsGeneralUseSplashScreenEnabled),
            _ => throw new InvalidDataException($"Invalid startup mode value: {value}")
        };
    }
}