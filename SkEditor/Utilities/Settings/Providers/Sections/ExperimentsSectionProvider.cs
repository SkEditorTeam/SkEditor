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
        return SettingSection.Create(TranslationKey.SettingsHomeExperiments)
            .WithIcon(Symbol.Beaker)
            .WithSettings(
            [
                InfoSetting
                    .Create(TranslatableString.FromRawString("Attention!"))
                    .WithDescription(
                        TranslatableString.FromRawString(
                            "These features are experimental and may cause issues. Use with caution!"))
                    .WithIcon(Symbol.Warning)
                    .WithType(InfoSetting.InfoType.Warning),

                ToggleSetting
                    .Create(TranslatableString.FromRawString("Auto-Completion Prototype"))
                    .WithDescription(
                        TranslatableString.FromRawString("Early prototype of auto completion, not very helpful."))
                    .WithIcon(Symbol.Wand)
                    .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.AutoCompletionPrototypeEnabled)),

                ToggleSetting
                    .Create(TranslatableString.FromRawString("Hex Preview"))
                    .WithDescription(TranslatableString.FromRawString("Preview hex colors in the editor."))
                    .WithIcon(Symbol.Color)
                    .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.HexPreviewEnabled))
            ]);
    }
}