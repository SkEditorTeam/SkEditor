using System;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SkEditor.Workspace.Remote.Models;

public partial class RemoteConnection : ObservableObject
{
    [ObservableProperty]
    public partial RemoteAuthMethod AuthMethod { get; set; } = RemoteAuthMethod.Password;

    [ObservableProperty]
    public partial string? CustomRootPath { get; set; }

    [ObservableProperty]
    public partial string Host { get; set; } = string.Empty;

    public Guid Id { get; set; } = Guid.NewGuid();

    [JsonIgnore]
    public bool IsAdded { get; set; } = true;

    [ObservableProperty]
    public partial string KeyFilePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EncryptedPassword { get; set; } = string.Empty;

    [JsonPropertyName("passwordHash")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OldPasswordHashAlias
    {
        get => null;
        set
        {
            if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(EncryptedPassword))
                EncryptedPassword = value;
        }
    }

    [ObservableProperty]
    public partial int Port { get; set; } = 22;

    [ObservableProperty]
    public partial string Username { get; set; } = string.Empty;

    public RemoteConnection()
    {
    }

    public RemoteConnection(RemoteConnectionInfo info)
    {
        Name = info.Name;
        Host = info.Host;
        Port = int.TryParse(info.Port, out int port) ? port : 22;
        Username = info.Username;
        AuthMethod = info.AuthMethod;
        KeyFilePath = info.KeyFilePath;
    }
}