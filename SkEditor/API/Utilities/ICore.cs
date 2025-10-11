using System;
using System.Threading.Tasks;
using SkEditor.Utilities;

namespace SkEditor.API;

/// <summary>
///     Interface for core utilities.
/// </summary>
public interface ICore
{
    /// <summary>
    ///     Get the application's configuration.
    /// </summary>
    /// <returns>The application's configuration.</returns>
    AppConfig GetAppConfig();

    /// <summary>
    ///     Get the application's version in a Version object.
    /// </summary>
    /// <returns>The application's version.</returns>
    Version GetAppVersion();

    /// <summary>
    ///     Get the application's informational version that may contain additional suffixes.
    /// </summary>
    /// <returns>The application's informational version.</returns>
    string GetInformationalVersion();

    /// <summary>
    ///     Get the arguments passed to the application at startup.
    /// </summary>
    /// <returns>An array of the startup arguments.</returns>
    string[] GetStartupArguments();

    /// <summary>
    ///     Set the arguments passed to the application at startup.
    /// </summary>
    void SetStartupArguments(string[]? args);

    /// <summary>
    ///     Gets a SkEditor's resource by key.
    /// </summary>
    /// <param name="key">The key of the resource.</param>
    /// <returns>The resource.</returns>
    object? GetApplicationResource(string key);

    /// <summary>
    ///     Open the desired web URL in the default browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    void OpenLink(string url);

    /// <summary>
    ///     Open the desired folder in the file explorer.
    /// </summary>
    void OpenFolder(string path);

    /// <summary>
    ///     Check if the developer mode is enabled or not.
    /// </summary>
    /// <returns>True if the developer mode is enabled, false otherwise.</returns>
    bool IsDeveloperMode();

    /// <summary>
    ///     Saves all the files to the temporary directory and saves the settings.
    ///     This method is called when the application crashes.
    /// </summary>
    Task SaveData();
}