using System.Text.Json.Serialization.Metadata;

namespace SkEditor.Utilities.Settings.Configs;

public class SecretsConfig : BaseConfig<SecretsConfig>
{
    protected override JsonTypeInfo<SecretsConfig> TypeInfo => AppJsonContext.Default.SecretsConfig;

    public override string FileName => "secrets.json";

    /// <summary>
    ///     The skUnity API key.
    /// </summary>
    public string SkUnityApiKey
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>
    ///     The Pastebin API key.
    /// </summary>
    public string PastebinApiKey
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>
    ///     The skript-mc.fr API key.
    /// </summary>
    public string SkriptMcApiKey
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    /// <summary>
    ///     The code.skript.pl API key.
    /// </summary>
    public string CodeSkriptPlApiKey
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    protected override void CopyFrom(SecretsConfig other)
    {
        SkUnityApiKey = other.SkUnityApiKey;
        PastebinApiKey = other.PastebinApiKey;
        SkriptMcApiKey = other.SkriptMcApiKey;
        CodeSkriptPlApiKey = other.CodeSkriptPlApiKey;
    }
}