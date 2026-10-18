using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Models.Remote;
using SkEditor.Utilities.Settings.Attributes;

namespace SkEditor.Utilities.Settings.Configs;

[SettingsConfig]
public partial class ConnectionsConfig : BaseConfig<ConnectionsConfig>
{
    [ObservableProperty]
    public partial ObservableCollection<RemoteConnection> Connections { get; set; } = [];

    protected override Environment.SpecialFolder TargetFolder => Environment.SpecialFolder.LocalApplicationData;
    protected override JsonTypeInfo<ConnectionsConfig> TypeInfo => AppJsonContext.Default.ConnectionsConfig;

    protected override string GetFileName()
    {
        return "connections.json";
    }
}