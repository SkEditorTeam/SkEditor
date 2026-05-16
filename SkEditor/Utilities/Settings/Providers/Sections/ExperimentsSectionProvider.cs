using FluentIcons.Common;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Localization;
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

                // ToggleSetting
                //     .Create(TranslationKey.SettingsExperimentsAutoCompletionPrototype)
                //     .WithDescription(TranslationKey.SettingsExperimentsAutoCompletionPrototypeDescription)
                //     .WithIcon(Symbol.Wand)
                //     .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.AutoCompletionPrototypeEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsExperimentsColorPreview)
                    .WithDescription(TranslationKey.SettingsExperimentsColorPreviewDescription)
                    .WithIcon(Symbol.Color)
                    .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.ColorPreviewEnabled))
            ]);
    }
}