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
        return SettingSection.Create("Experiments", TranslationKey.SettingsSectionExperiments)
            .WithIcon(Symbol.Beaker)
            .WithSettings(
            [
                InfoSetting
                    .Create(TranslationKey.Attention)
                    .WithDescription(TranslationKey.SettingsExperimentsWarningDescription)
                    .WithIcon(Symbol.Warning)
                    .WithType(InfoSetting.InfoType.Warning),


                ExpandableSetting
                    .Create(TranslationKey.SettingsExperimentsColorPreview)
                    .WithDescription(TranslationKey.SettingsExperimentsColorPreviewDescription)
                    .WithIcon(Symbol.Color)
                    .WithChildren(
                        ToggleSetting
                            .Create(TranslationKey.SettingsExperimentsColorPreviewOption)
                            .WithDescription(TranslationKey.SettingsExperimentsColorPreviewOptionDescription)
                            .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.ColorPreviewEnabled)),
                        ToggleSetting
                            .Create(TranslationKey.SettingsExperimentsColorPreviewGlobal)
                            .WithDescription(TranslationKey.SettingsExperimentsColorPreviewGlobalDescription)
                            .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.GlobalColorPreviewEnabled))
                    ),

                ToggleSetting
                    .Create(TranslatableString.FromRawString("Split View"))
                    .WithDescription(TranslatableString.FromRawString(
                        "Enables the Split View feature. THIS FEATURE IS VERY EARLY IN DEVELOPMENT!"))
                    .WithIcon(Symbol.DocumentLandscapeSplitHint)
                    .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.SplitViewEnabled)),

                ToggleSetting
                    .Create(TranslatableString.FromRawString("Bottom Drawer"))
                    .WithDescription(TranslatableString.FromRawString(
                        "Enables the Bottom Drawer feature. THIS FEATURE IS VERY EARLY IN DEVELOPMENT!"))
                    .WithIcon(Symbol.PanelBottom)
                    .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.BottomDrawerEnabled))
            ]);
    }
}