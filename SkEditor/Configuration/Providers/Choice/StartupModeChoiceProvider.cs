using System.Collections.Generic;
using System.IO;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Startup;

namespace SkEditor.Utilities.Settings.Providers.Choice;

public class StartupModeChoiceProvider : ISettingsChoiceProvider<StartupMode>
{
    public IEnumerable<StartupMode> GetValues()
    {
        return [StartupMode.Auto, StartupMode.DisableSplashScreen, StartupMode.EnableSplashScreen];
    }

    public TranslatableString GetDisplayValue(StartupMode value)
    {
        return value switch
        {
            StartupMode.Auto => TranslationKey.SettingsGeneralUseSplashScreenAuto,
            StartupMode.DisableSplashScreen => TranslationKey.SettingsGeneralUseSplashScreenDisabled,
            StartupMode.EnableSplashScreen => TranslationKey.SettingsGeneralUseSplashScreenEnabled,
            _ => throw new InvalidDataException($"Invalid startup mode value: {value}")
        };
    }
}