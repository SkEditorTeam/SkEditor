using System.Threading.Tasks;
using SkEditor.Utilities.Settings;
using AppSettings = SkEditor.Utilities.Settings.Configs.AppSettings;
using BinarySettings = SkEditor.Utilities.Settings.Configs.BinarySettings;
using SecretsConfig = SkEditor.Utilities.Settings.Configs.SecretsConfig;
using StateConfig = SkEditor.Utilities.Settings.Configs.StateConfig;

namespace SkEditor.API;

/// <summary>
///     Interface for managing application configurations.
/// </summary>
public interface IConfigs
{
    /// <summary>
    ///     Get the application data path.
    /// </summary>
    /// <returns>The application data path.</returns>
    string GetAppDataPath();

    /// <summary>
    ///     Load all configuration files asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LoadAllConfigsAsync();

    /// <summary>
    ///     Save all configuration files asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveAllConfigsAsync();
    
    /// <summary>
    ///     Save all configuration files immediately asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveAllConfigsImmediateAsync();

    /// <summary>
    ///     Check and migrate old configuration files to the new format asynchronously.
    /// </summary>
    /// <returns>>A task representing the asynchronous operation.</returns>
    Task MigrateConfigsAsync();

    /// <summary>
    ///     Get the general application settings.
    /// </summary>
    /// <returns>The application settings.</returns>
    AppSettings GetAppSettings();

    /// <summary>
    ///     Get the secrets configuration.
    /// </summary>
    /// <returns>The secrets configuration.</returns>
    /// <remarks>Used to store sensitive data like API keys.</remarks>
    SecretsConfig GetSecretsConfig();

    /// <summary>
    ///     Get the experiments configuration.
    /// </summary>
    /// <returns>The experiments configuration.</returns>
    /// <remarks>Used to store feature flags for experimental features.</remarks>
    ExperimentsConfig GetExperimentsConfig();

    /// <summary>
    ///     Get the state configuration.
    /// </summary>
    /// <returns>The state configuration.</returns>
    StateConfig GetStateConfig();

    /// <summary>
    ///     Get the binary settings.
    /// </summary>
    /// <returns>The binary settings.</returns>
    /// <remarks>Used for a quick access of low-level settings like splash screen or first run.</remarks>
    BinarySettings GetBinarySettings();

    /// <summary>
    ///     Get a specific configuration by type.
    /// </summary>
    /// <typeparam name="T">The type of configuration to retrieve.</typeparam>
    /// <returns>The requested configuration.</returns>
    T? GetConfig<T>() where T : IConfig;

    /// <summary>
    ///     Add a new configuration to the system.
    /// </summary>
    /// <param name="config">The configuration to add.</param>
    void AddConfig<T>(T config) where T : IConfig;

    /// <summary>
    ///     Remove a configuration from the system by type.
    /// </summary>
    /// <typeparam name="T">The type of configuration to remove.</typeparam>
    void RemoveConfig<T>() where T : IConfig;
}