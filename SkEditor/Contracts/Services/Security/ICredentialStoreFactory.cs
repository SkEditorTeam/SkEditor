using SkEditor.Utilities.Security;

namespace SkEditor.Contracts.Services.Security;

public interface ICredentialStoreFactory
{
    ICredentialStore Create();
}