using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Models;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class ExperimentsSectionProvider : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        return SettingSection.Create(TranslationKey.SettingsHomeExperiments)
            .WithIcon(Symbol.Beaker)
            .WithSettings(
            [
            ]);
    }
}