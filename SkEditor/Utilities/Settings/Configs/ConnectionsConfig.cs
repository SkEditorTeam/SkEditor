using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Models.Remote;
using SkEditor.Utilities.Settings.Attributes;

namespace SkEditor.Utilities.Settings.Configs;

[SettingsConfig]
public partial class ConnectionsConfig : BaseConfig<ConnectionsConfig>
{
    protected override JsonTypeInfo<ConnectionsConfig> TypeInfo => AppJsonContext.Default.ConnectionsConfig;

    [ObservableProperty]
    public partial List<RemoteConnection> Connections { get; set; } = [];

    protected override string GetFileName()
    {
        return "connections.json";
    }
}