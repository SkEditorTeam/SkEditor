using FluentIcons.Common;
using R3;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using ExperimentsProperty = SkEditor.Utilities.Settings.ExperimentsConfig.ExperimentsConfigProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class AnalyzerSectionProvider(
    ExperimentsConfig experiments,
    AppSettings appSettings) : ISettingsSectionProvider
{
    public string ParentId => "Editor";

    public SettingSection GetSection()
    {
        SettingSection section = SettingSection.Create("Analyzer", "Analyzer".ToTranslatable())
            .WithIcon(Symbol.DocumentSearch)
            .WithSettings(
            [
                InfoSetting
                    .Create(TranslationKey.Attention)
                    .WithDescription(
                        "The Analyzer is an experimental feature and can be toggled in the Experiments section."
                            .ToTranslatable())
                    .WithIcon(Symbol.Warning)
                    .WithType(InfoSetting.InfoType.Warning),
                
                ToggleSetting
                    .Create("Show scrollbar markers".ToTranslatable())
                    .WithDescription(
                        "Displays colored error and warning indicators directly on the editor scrollbar."
                            .ToTranslatable())
                    .WithIcon(Symbol.SidebarSearch)
                    .WithProperty(
                        experiments.GetProperty<bool>(ExperimentsProperty.AnalyzerErrorScrollMarkersEnabled)),
                ToggleSetting
                    .Create("Diagnostic tooltips".ToTranslatable())
                    .WithDescription("Shows a tooltip with issue details when you hover over code errors."
                        .ToTranslatable())
                    .WithIcon(Symbol.CommentNote)
                    .WithProperty(
                        experiments.GetProperty<bool>(ExperimentsProperty.AnalyzerErrorTooltipsEnabled))
            ]);

        section.IsVisible = experiments
            .ObservePropertyChanged(x => x.AnalyzerEnabled)
            .ToBindableReactiveProperty();

        return section;
    }
}