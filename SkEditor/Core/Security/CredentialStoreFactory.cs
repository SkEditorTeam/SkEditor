using System;
using System.IO;
using SkEditor.Configuration;

namespace SkEditor.Core.Security;

public class CredentialStoreFactory(IConfigService configs) : ICredentialStoreFactory
{
    public ICredentialStore Create()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsCredentialStore();
        }

        string keyPath = Path.Combine(configs.GetLocalAppDataPath(), "master.key");
        MigrateKeyFolder(keyPath);

        return new UnixCredentialStore(keyPath);
    }

    // TEMP: Remove migration from Roaming when leaving Alpha
    private void MigrateKeyFolder(string keyPath)
    {
        string legacyKeyPath = Path.Combine(configs.GetAppDataPath(), "master.key");

        if (File.Exists(keyPath) || !File.Exists(legacyKeyPath))
        {
            return;
        }

        try
        {
            string? dir = Path.GetDirectoryName(keyPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.Move(legacyKeyPath, keyPath, true);
        }
        catch
        {
            // ignored
        }
    }
}