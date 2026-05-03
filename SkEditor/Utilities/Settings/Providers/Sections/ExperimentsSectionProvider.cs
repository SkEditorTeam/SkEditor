using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Models;
using SkEditor.Utilities.Settings.Models.Types;
using ExperimentsProperty = SkEditor.Utilities.Settings.ExperimentsConfig.ExperimentsConfigProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class ExperimentsSectionProvider(ExperimentsConfig experiments) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        return SettingSection.Create(TranslationKey.SettingsSectionExperiments)
            .WithIcon(Symbol.Beaker)
            .WithSettings(
            [
                InfoSetting
                    .Create(TranslationKey.Attention)
                    .WithDescription(TranslationKey.SettingsExperimentsWarningDescription)
                    .WithIcon(Symbol.Warning)
                    .WithType(InfoSetting.InfoType.Warning),

                ToggleSetting
                    .Create(TranslationKey.SettingsExperimentsAutoCompletionPrototype)
                    .WithDescription(TranslationKey.SettingsExperimentsAutoCompletionPrototypeDescription)
                    .WithIcon(Symbol.Wand)
                    .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.AutoCompletionPrototypeEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsExperimentsHexPreview)
                    .WithDescription(TranslationKey.SettingsExperimentsHexPreviewDescription)
                    .WithIcon(Symbol.Color)
                    .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.HexPreviewEnabled))
            ]);
    }
}