using System;
using System.IO;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Security;
using SkEditor.Utilities.Security;

namespace SkEditor.Services.Security;

public class CredentialStoreFactory(IConfigService configs) : ICredentialStoreFactory
{
    public ICredentialStore Create()
    {
        if (OperatingSystem.IsWindows())
        {
            return new WindowsCredentialStore();
        }

        string keyPath = Path.Combine(configs.GetAppDataPath(), "master.key");

        return new UnixCredentialStore(keyPath);
    }
}