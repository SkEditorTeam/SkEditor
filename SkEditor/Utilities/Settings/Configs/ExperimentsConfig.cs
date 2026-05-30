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

    protected override string GetFileName()
    {
        return "experiments.json";
    }
}