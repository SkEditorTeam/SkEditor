using System;
using System.Diagnostics.CodeAnalysis;

namespace SkEditor.Workspace.Projects;

public record SftpConnectionInfo(
    string Host,
    int Port,
    string? Username,
    string? Password,
    string? RootPath
);

public static class SftpConnectionStringParser
{
    public static bool TryParse(string connectionString, [NotNullWhen(true)] out SftpConnectionInfo? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        if (!Uri.TryCreate(connectionString, UriKind.Absolute, out Uri? uri))
        {
            return false;
        }

        if (!uri.Scheme.Equals("sftp", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (string.IsNullOrEmpty(uri.Host))
        {
            return false;
        }

        string? username = null;
        string? password = null;

        if (!string.IsNullOrEmpty(uri.UserInfo))
        {
            string[] userInfo = uri.UserInfo.Split(':', 2);
            username = Uri.UnescapeDataString(userInfo[0]);
            password = userInfo.Length > 1
                ? Uri.UnescapeDataString(userInfo[1])
                : null;
        }

        int port = uri.Port == -1 ? 22 : uri.Port;
        string rootPath = string.IsNullOrEmpty(uri.AbsolutePath) ? "/" : uri.AbsolutePath;

        result = new SftpConnectionInfo(uri.Host, port, username, password, rootPath);
        return true;
    }
}