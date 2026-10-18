using System.Text.Json.Serialization;
using SkEditor.Configuration.Configs;

namespace SkEditor.Configuration;

[JsonSerializable(typeof(AppSettings))]
[JsonSerializable(typeof(SecretsConfig))]
[JsonSerializable(typeof(ExperimentsConfig))]
[JsonSerializable(typeof(StateConfig))]
[JsonSerializable(typeof(ConnectionsConfig))]
[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class AppJsonContext : JsonSerializerContext;