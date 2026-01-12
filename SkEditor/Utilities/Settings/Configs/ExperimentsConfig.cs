using System.Text.Json.Serialization.Metadata;
using SkEditor.Utilities.Settings.Attributes;

namespace SkEditor.Utilities.Settings;

[SettingsConfig]
public partial class ExperimentsConfig : BaseConfig<ExperimentsConfig>
{
    protected override JsonTypeInfo<ExperimentsConfig> TypeInfo => AppJsonContext.Default.ExperimentsConfig;
    public override string FileName => "experiments.json";

    /// <summary>
    ///     Enables the Auto-Completion Prototype feature.
    /// </summary>
    public bool AutoCompletionPrototypeEnabled
    {
        get;
        set => SetProperty(ref field, value);
    } = false;

    /// <summary>
    ///     Enables the Projects feature.
    /// </summary>
    public bool ProjectsEnabled
    {
        get;
        set => SetProperty(ref field, value);
    } = false;

    /// <summary>
    ///     Enables the Hex Preview feature.
    /// </summary>
    public bool HexPreviewEnabled
    {
        get;
        set => SetProperty(ref field, value);
    } = false;

    /// <summary>
    ///     Enables the Code Parser feature.
    /// </summary>
    public bool CodeParserEnabled
    {
        get;
        set => SetProperty(ref field, value);
    } = false;

    /// <summary>
    ///     Makes the Code Parser run in real-time as you type.
    /// </summary>
    public bool RealtimeCodeParserEnabled
    {
        get;
        set => SetProperty(ref field, value);
    } = false;

    /// <summary>
    ///     Enables the Folding feature.
    /// </summary>
    public bool FoldingEnabled
    {
        get;
        set => SetProperty(ref field, value);
    } = false;

    /// <summary>
    ///     Enables the Session Restoring feature.
    /// </summary>
    public bool SessionRestoringEnabled
    {
        get;
        set => SetProperty(ref field, value);
    } = false;
}