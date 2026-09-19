using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using R3;
using SkEditor.Models.Messages;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using ExperimentsProperty = SkEditor.Utilities.Settings.ExperimentsConfig.ExperimentsConfigProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class ExperimentsSectionProvider(ExperimentsConfig experiments, IMessenger messenger) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        return SettingSection.Create("Experiments", TranslationKey.SettingsSectionExperiments)
            .WithIcon(Symbol.Beaker)
            .WithOrder(40)
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
                    .Create(TranslationKey.SettingsExperimentsSplitView)
                    .WithDescription(TranslationKey.SettingsExperimentsSplitViewDescription)
                    .WithIcon(Symbol.DocumentLandscapeSplitHint)
                    .WithChildren(
                        InfoSetting
                            .Create(TranslationKey.SettingsExperimentsSplitViewWarningTitle)
                            .WithDescription(TranslationKey.SettingsExperimentsSplitViewWarningDescription)
                            .WithIcon(Symbol.Warning)
                            .WithType(InfoSetting.InfoType.Warning),
                        ToggleSetting
                            .Create(TranslationKey.SettingsExperimentsSplitViewOption)
                            .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.SplitViewEnabled))
                    ),

                ExpandableSetting
                    .Create(TranslationKey.SettingsExperimentsAnalyzer)
                    .WithDescription(TranslationKey.SettingsExperimentsAnalyzerDescription)
                    .WithIcon(Symbol.DocumentSearch)
                    .WithChildren(
                        InfoSetting
                            .Create(TranslationKey.SettingsExperimentsAnalyzerWarningTitle)
                            .WithDescription(TranslationKey.SettingsExperimentsAnalyzerWarningDescription)
                            .WithIcon(Symbol.Warning)
                            .WithType(InfoSetting.InfoType.Warning),
                        ToggleSetting
                            .Create(TranslationKey.SettingsExperimentsAnalyzerOption)
                            .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.AnalyzerEnabled)),
                        ClickableActionSetting
                            .Create(TranslationKey.SettingsExperimentsAnalyzerOpenSettings)
                            .WithAction(() => messenger.Send(new NavigateToSettingsSectionMessage("Analyzer")))
                            .WithActionIcon(Symbol.ChevronRight)
                            .WithEnabled(experiments.ObservePropertyChanged(x => x.AnalyzerEnabled)
                                .ToBindableReactiveProperty())
                    ),

                ExpandableSetting
                    .Create(TranslationKey.SettingsExperimentsTerminal)
                    .WithDescription(TranslationKey.SettingsExperimentsTerminalDescription)
                    .WithIcon(Symbol.WindowConsole)
                    .WithChildren(
                        InfoSetting
                            .Create(TranslationKey.SettingsExperimentsTerminalWarningTitle)
                            .WithDescription(TranslationKey.SettingsExperimentsTerminalWarningDescription)
                            .WithIcon(Symbol.Warning)
                            .WithType(InfoSetting.InfoType.Warning),
                        ToggleSetting
                            .Create(TranslationKey.SettingsExperimentsTerminalOption)
                            .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.TerminalEnabled))
                    ),

                ExpandableSetting
                    .Create("Code completion".ToTranslatableWip())
                    .WithDescription("Displays suggestions while writing code.".ToTranslatableWip())
                    .WithIcon(Symbol.TextBulletListSquareSparkle)
                    .WithChildren(
                        InfoSetting
                            .Create("Just a prototype".ToTranslatableWip())
                            .WithDescription("This feature is an early WIP and you shouldn't expect much from it."
                                .ToTranslatableWip())
                            .WithIcon(Symbol.Warning)
                            .WithType(InfoSetting.InfoType.Warning),
                        ToggleSetting
                            .Create("Enable code completion".ToTranslatableWip())
                            .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.CodeCompletionEnabled))
                    )
            ]);
    }
}