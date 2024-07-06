using SkEditor.Utilities;
using System;

namespace SkEditor.API;

/// <summary>
/// Interface for core utilities.
/// </summary>
public interface ICore
{

    /// <summary>
    /// Get the application's configuration.
    /// </summary>
    /// <returns>The application's configuration.</returns>
    public AppConfig GetAppConfig();

    /// <summary>
    /// Get the application's version in a Version object.
    /// </summary>
    /// <returns>The application's version.</returns>
    public Version GetAppVersion();

    /// <summary>
    /// Get the application's informational version that may contain additional suffixes.
    /// </summary>
    /// <returns>The application's informational version.</returns>
    public string GetInformationalVersion();

    /// <summary>
    /// Get the arguments passed to the application at startup.
    /// </summary>
    /// <returns>An array of the startup arguments.</returns>
    public string[] GetStartupArguments();

    /// <summary>
    /// Set the arguments passed to the application at startup.
    /// </summary>
    public void SetStartupArguments(string[]? args);

    /// <summary>
    /// Gets a SkEditor's resource by key.
    /// </summary>
    /// <param name="key">The key of the resource.</param>
    /// <returns>The resource.</returns>
    public object? GetApplicationResource(string key);

    /// <summary>
    /// Open the desired web URL in the default browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    public void OpenLink(string url);

    /// <summary>
    /// Open the desired folder in the file explorer.
    /// </summary>
    public void OpenFolder(string path);

    /// <summary>
    /// Check if the developer mode is enabled or not.
    /// </summary>
    /// <returns>True if the developer mode is enabled, false otherwise.</returns>
    public bool IsDeveloperMode();

    /// <summary>
    /// Saves all the files to the temporary directory and saves the settings.
    /// This method is called when the application crashes.
    /// </summary>
    public void SaveData();
}