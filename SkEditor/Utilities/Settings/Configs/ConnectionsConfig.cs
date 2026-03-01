using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
using SkEditor.Models.Remote;
using SkEditor.Utilities.Settings.Attributes;

namespace SkEditor.Utilities.Settings.Configs;

[SettingsConfig]
public partial class ConnectionsConfig : BaseConfig<ConnectionsConfig>
{
    protected override JsonTypeInfo<ConnectionsConfig> TypeInfo => AppJsonContext.Default.ConnectionsConfig;

    public override string FileName => "connections.json";

    public List<RemoteConnection> Connections
    {
        get;
        set => SetProperty(ref field, value);
    } = [];
}