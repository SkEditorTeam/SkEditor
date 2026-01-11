using System.Text.Json.Serialization.Metadata;

namespace SkEditor.Utilities.Settings;

public class ExperimentsConfig : BaseConfig<ExperimentsConfig>
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

    protected override void CopyFrom(ExperimentsConfig other)
    {
        AutoCompletionPrototypeEnabled = other.AutoCompletionPrototypeEnabled;
        ProjectsEnabled = other.ProjectsEnabled;
        HexPreviewEnabled = other.HexPreviewEnabled;
        CodeParserEnabled = other.CodeParserEnabled;
        RealtimeCodeParserEnabled = other.RealtimeCodeParserEnabled;
        FoldingEnabled = other.FoldingEnabled;
        SessionRestoringEnabled = other.SessionRestoringEnabled;
    }
}