using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace SkEditor.Core.Security;

[UnsupportedOSPlatform("windows")]
public class UnixCredentialStore : ICredentialStore
{
    private readonly byte[] _key;

    public UnixCredentialStore(string keyPath)
    {
        if (!File.Exists(keyPath))
        {
            _key = new byte[32];
            RandomNumberGenerator.Fill(_key);

            string? directory = Path.GetDirectoryName(keyPath);
            if (directory == null)
            {
                throw new InvalidOperationException($"Invalid key path: {keyPath}");
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(keyPath, _key);

            File.SetUnixFileMode(keyPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
        else
        {
            _key = File.ReadAllBytes(keyPath);
        }
    }

    public string Protect(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        byte[] result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Unprotect(string encryptedText)
    {
        byte[] fullCipher = Convert.FromBase64String(encryptedText);

        using Aes aes = Aes.Create();
        aes.Key = _key;

        byte[] iv = new byte[aes.BlockSize / 8];
        byte[] cipherBytes = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;

        using ICryptoTransform decryptor = aes.CreateDecryptor();
        byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}