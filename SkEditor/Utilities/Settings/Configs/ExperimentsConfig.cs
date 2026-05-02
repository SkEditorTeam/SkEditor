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
    ///     Enables the Hex Preview feature.
    /// </summary>
    [ObservableProperty]
    public partial bool HexPreviewEnabled { get; set; } = false;

    protected override string GetFileName()
    {
        return "experiments.json";
    }
}