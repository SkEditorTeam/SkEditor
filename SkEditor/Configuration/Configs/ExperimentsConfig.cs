using System.Text.Json.Serialization.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Configuration.Abstractions;

namespace SkEditor.Configuration.Configs;

[SettingsConfig]
public partial class ExperimentsConfig : BaseConfig<ExperimentsConfig>
{
    /// <summary>
    ///     Enables the Analyzer.
    /// </summary>
    [ObservableProperty]
    public partial bool AnalyzerEnabled { get; set; } = false;

    /// <summary>
    ///     Enables the Analyzer error markers visible on the scrollbar.
    /// </summary>
    [ObservableProperty]
    public partial bool AnalyzerErrorScrollMarkersEnabled { get; set; } = true;

    /// <summary>
    ///     Enables the Analyzer tooltips shown when hovering on an error.
    /// </summary>
    [ObservableProperty]
    public partial bool AnalyzerErrorTooltipsEnabled { get; set; } = true;

    /// <summary>
    ///     Enables the Split View feature.
    /// </summary>
    [ObservableProperty]
    public partial bool SplitViewEnabled { get; set; } = false;

    /// <summary>
    ///     Enables the Terminal.
    /// </summary>
    [ObservableProperty]
    public partial bool TerminalEnabled { get; set; } = false;

    /// <summary>
    ///     Enables the Code Completion.
    /// </summary>
    [ObservableProperty]
    public partial bool CodeCompletionEnabled { get; set; } = false;

    protected override JsonTypeInfo<ExperimentsConfig> TypeInfo => AppJsonContext.Default.ExperimentsConfig;

    protected override string GetFileName()
    {
        return "experiments.json";
    }
}