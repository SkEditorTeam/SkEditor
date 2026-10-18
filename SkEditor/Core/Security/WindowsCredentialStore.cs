using System;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace SkEditor.Core.Security;

[SupportedOSPlatform("windows")]
public class WindowsCredentialStore : ICredentialStore
{
    public string Protect(string plainText)
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encryptedBytes);
    }

    public string Unprotect(string encryptedText)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(plainBytes);
    }
}