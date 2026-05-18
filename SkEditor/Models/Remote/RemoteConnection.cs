using System;

namespace SkEditor.Models.Remote;

public class RemoteConnection
{
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

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    public string Username { get; set; } = string.Empty;

    public RemoteAuthMethod AuthMethod { get; set; } = RemoteAuthMethod.Password;

    public string KeyFilePath { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}