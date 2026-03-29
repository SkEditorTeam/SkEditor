namespace SkEditor.Contracts.Services;

public interface IDiscordIntegrationService
{
    /// <summary>
    ///     Initializes the Discord RPC integration. This method should be called when the application starts to connect to
    ///     Discord and set up all the info.
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Uninitializes the Discord RPC integration, clears the status and cleans up the resources.
    /// </summary>
    void Uninitialize();
}