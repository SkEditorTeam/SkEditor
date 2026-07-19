using FluentIcons.Common;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Extensions;
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

                ExpandableSetting
                    .Create("Split View".ToTranslatable())
                    .WithDescription(
                        "Allows you to split the editor into two views for better multitasking and comparison of code files."
                            .ToTranslatable())
                    .WithIcon(Symbol.DocumentLandscapeSplitHint)
                    .WithChildren(
                        InfoSetting
                            .Create("Highly experimental!".ToTranslatable())
                            .WithDescription(
                                "This is more of a prototype than a fully functional feature. It *will* probably not work as expected and may cause issues."
                                    .ToTranslatable())
                            .WithIcon(Symbol.Warning)
                            .WithType(InfoSetting.InfoType.Warning),
                        ToggleSetting
                            .Create("Enable Split View".ToTranslatable())
                            .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.SplitViewEnabled))
                    ),

                ExpandableSetting
                    .Create("Analyzer".ToTranslatable())
                    .WithDescription(
                        "Enables the Analyzer feature, which provides real-time code analysis and suggestions."
                            .ToTranslatable())
                    .WithIcon(Symbol.Search)
                    .WithChildren(
                        InfoSetting
                            .Create("Important!".ToTranslatable())
                            .WithDescription("Restart the app after toggling this setting to take effect."
                                .ToTranslatable())
                            .WithIcon(Symbol.Warning)
                            .WithType(InfoSetting.InfoType.Warning),
                        ToggleSetting
                            .Create("Enable Analyzer".ToTranslatable())
                            .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.AnalyzerEnabled))
                        // ClickableActionSetting
                        //     .Create("Go to Analyzer settings".ToTranslatable())
                        //     .WithAction(() => messenger.Send(new NavigateToSettingsSectionMessage("Analyzer")))
                        //     .WithEnabled(experiments.ObservePropertyChanged(x => x.AnalyzerEnabled)
                        //         .ToBindableReactiveProperty())
                    ),

                ExpandableSetting
                    .Create("Terminal".ToTranslatable())
                    .WithDescription(
                        "Gives you access to an integrated terminal within the editor for running commands and scripts."
                            .ToTranslatable())
                    .WithIcon(Symbol.WindowConsole)
                    .WithChildren(
                        InfoSetting
                            .Create("Highly experimental!".ToTranslatable())
                            .WithDescription(
                                "The terminal view is still in development and has some serious bugs, i.e. scrolling issues."
                                    .ToTranslatable())
                            .WithIcon(Symbol.Warning)
                            .WithType(InfoSetting.InfoType.Warning),
                        ToggleSetting
                            .Create("Enable Terminal".ToTranslatable())
                            .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.TerminalEnabled))
                    )
            ]);
    }
}