using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using R3;
using SkEditor.Configuration.Configs;
using SkEditor.Configuration.Messages;
using SkEditor.Configuration.Models;
using SkEditor.Configuration.Models.Types;
using SkEditor.Core.Localization;
using ExperimentsProperty = SkEditor.Configuration.Configs.ExperimentsConfig.ExperimentsConfigProperty;

namespace SkEditor.Configuration.Providers.Sections;

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
                    .Create(TranslationKey.SettingsExperimentsCodeCompletion)
                    .WithDescription(TranslationKey.SettingsExperimentsCodeCompletionDescription)
                    .WithIcon(Symbol.TextBulletListSquareSparkle)
                    .WithChildren(
                        InfoSetting
                            .Create(TranslationKey.SettingsExperimentsCodeCompletionWarningTitle)
                            .WithDescription(TranslationKey.SettingsExperimentsCodeCompletionWarningDescription)
                            .WithIcon(Symbol.Warning)
                            .WithType(InfoSetting.InfoType.Warning),
                        ToggleSetting
                            .Create(TranslationKey.SettingsExperimentsCodeCompletionOption)
                            .WithProperty(experiments.GetProperty<bool>(ExperimentsProperty.CodeCompletionEnabled))
                    )
            ]);
    }
}