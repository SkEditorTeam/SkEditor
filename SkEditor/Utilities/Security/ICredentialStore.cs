namespace SkEditor.Utilities.Security;

public interface ICredentialStore
{
    string Protect(string plainText);
    string Unprotect(string encryptedText);
}