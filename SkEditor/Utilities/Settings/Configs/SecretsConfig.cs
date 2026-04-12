using System.Text.Json.Serialization.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Settings.Attributes;

namespace SkEditor.Utilities.Settings.Configs;

[SettingsConfig]
public partial class SecretsConfig : BaseConfig<SecretsConfig>
{
    protected override JsonTypeInfo<SecretsConfig> TypeInfo => AppJsonContext.Default.SecretsConfig;

    /// <summary>
    ///     The skUnity API key.
    /// </summary>
    [ObservableProperty]
    public partial string SkUnityApiKey { get; set; } = string.Empty;

    /// <summary>
    ///     The Pastebin API key.
    /// </summary>
    [ObservableProperty]
    public partial string PastebinApiKey { get; set; } = string.Empty;

    /// <summary>
    ///     The skript-mc.fr API key.
    /// </summary>
    [ObservableProperty]
    public partial string SkriptMcApiKey { get; set; } = string.Empty;

    /// <summary>
    ///     The code.skript.pl API key.
    /// </summary>
    [ObservableProperty]
    public partial string CodeSkriptPlApiKey { get; set; } = string.Empty;

    protected override string GetFileName()
    {
        return "secrets.json";
    }
}