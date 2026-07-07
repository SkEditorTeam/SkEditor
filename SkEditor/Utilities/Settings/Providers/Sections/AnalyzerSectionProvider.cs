using FluentIcons.Common;
using R3;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class AnalyzerSectionProvider(
    ExperimentsConfig experiments,
    AppSettings appSettings) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        SettingSection section = SettingSection.Create("Analyzer", "Analyzer".ToTranslatable())
            .WithIcon(Symbol.Search)
            .WithSettings(
            [
                ToggleSetting
                    .Create("lol".ToTranslatable())
                    .WithDescription("lol_desc".ToTranslatable())
                    .WithIcon(Symbol.Info)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettings.AppSettingsProperty.IsDevModeEnabled))
            ]);

        section.IsVisible = experiments
            .ObservePropertyChanged(x => x.AnalyzerEnabled)
            .ToBindableReactiveProperty();

        return section;
    }
}