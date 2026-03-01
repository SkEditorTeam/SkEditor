using SkEditor.API;
using SkEditor.Contracts.Services;

namespace SkEditor.Services;

public class RemoteConnectionService : IRemoteConnectionService
{
    public void Test()
    {
        SkEditorAPI.Logs.Info($"Testing {nameof(RemoteConnectionService)}");
    }
}