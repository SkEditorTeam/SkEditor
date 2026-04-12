using System.Text.Json.Serialization.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Settings.Attributes;

namespace SkEditor.Utilities.Settings;

[SettingsConfig]
public partial class ExperimentsConfig : BaseConfig<ExperimentsConfig>
{
    protected override JsonTypeInfo<ExperimentsConfig> TypeInfo => AppJsonContext.Default.ExperimentsConfig;

    /// <summary>
    ///     Enables the Auto-Completion Prototype feature.
    /// </summary>
    [ObservableProperty]
    public partial bool AutoCompletionPrototypeEnabled { get; set; } = false;

    /// <summary>
    ///     Enables the Projects feature.
    /// </summary>
    [ObservableProperty]
    public partial bool ProjectsEnabled { get; set; } = false;

    /// <summary>
    ///     Enables the Hex Preview feature.
    /// </summary>
    [ObservableProperty]
    public partial bool HexPreviewEnabled { get; set; } = false;

    /// <summary>
    ///     Enables the Code Parser feature.
    /// </summary>
    [ObservableProperty]
    public partial bool CodeParserEnabled { get; set; } = false;

    /// <summary>
    ///     Makes the Code Parser run in real-time as you type.
    /// </summary>
    [ObservableProperty]
    public partial bool RealtimeCodeParserEnabled { get; set; } = false;

    /// <summary>
    ///     Enables the Folding feature.
    /// </summary>
    [ObservableProperty]
    public partial bool FoldingEnabled { get; set; } = false;

    /// <summary>
    ///     Enables the Session Restoring feature.
    /// </summary>
    [ObservableProperty]
    public partial bool SessionRestoringEnabled { get; set; } = false;

    protected override string GetFileName()
    {
        return "experiments.json";
    }
}