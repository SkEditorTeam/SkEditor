using System.Text.Json.Serialization.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Settings.Attributes;

namespace SkEditor.Utilities.Settings;

[SettingsConfig]
public partial class ExperimentsConfig : BaseConfig<ExperimentsConfig>
{
    protected override JsonTypeInfo<ExperimentsConfig> TypeInfo => AppJsonContext.Default.ExperimentsConfig;

    /// <summary>
    ///     Enables the Color Preview feature.
    /// </summary>
    [ObservableProperty]
    public partial bool ColorPreviewEnabled { get; set; } = false;

    /// <summary>
    ///     Whether the Color Preview feature should be enabled for all the file types or just Skript files
    /// </summary>
    [ObservableProperty]
    public partial bool GlobalColorPreviewEnabled { get; set; } = false;

    /// <summary>
    ///     Enables the Split View feature.
    /// </summary>
    [ObservableProperty]
    public partial bool SplitViewEnabled { get; set; } = false;

    protected override string GetFileName()
    {
        return "experiments.json";
    }
}