using System.Threading.Tasks;
using SkEditor.Utilities.Settings;

namespace SkEditor.Contracts.Services;

public interface IConfigService
{
    /// <summary>
    ///     Get the application data path.
    /// </summary>
    string GetAppDataPath();

    /// <summary>
    ///     Get the local application data path.
    /// </summary>
    string GetLocalAppDataPath();

    /// <summary>
    ///     Load all configuration files asynchronously.
    /// </summary>
    Task LoadAllConfigsAsync();

    /// <summary>
    ///     Save all configuration files asynchronously.
    /// </summary>
    Task SaveAllConfigsAsync();

    /// <summary>
    ///     Save all configuration files immediately asynchronously.
    /// </summary>
    Task SaveAllConfigsImmediateAsync();

    /// <summary>
    ///     Get a specific configuration of type T.
    /// </summary>
    /// <param name="config">The configuration to add.</param>
    /// <typeparam name="T">The type of the configuration.</typeparam>
    void AddConfig<T>(T config) where T : IConfig;

    /// <summary>
    ///     Get a specific configuration of type T.
    /// </summary>
    /// <typeparam name="T">The type of the configuration.</typeparam>
    void RemoveConfig<T>() where T : IConfig;

    /// <summary>
    ///     Get a specific configuration of type T.
    /// </summary>
    /// <typeparam name="T">The type of the configuration.</typeparam>
    /// <returns>The configuration of type T, or null if it doesn't exist.</returns>
    T? GetConfig<T>() where T : IConfig;
}