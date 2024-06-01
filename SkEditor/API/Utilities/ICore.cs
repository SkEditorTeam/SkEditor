using System;
using SkEditor.Utilities;

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
    /// Get the application's version.
    /// </summary>
    /// <returns>The application's version.</returns>
    public Version GetAppVersion();

    /// <summary>
    /// Get the arguments passed to the application at startup.
    /// </summary>
    /// <returns>An array of the startup arguments.</returns>
    public string[] GetStartupArguments();

    /// <summary>
    /// Get an SkEditor's resource by key.
    /// </summary>
    /// <param name="key">The key of the resource.</param>
    /// <returns>The resource.</returns>
    public object? GetApplicationResource(string key);

    /// <summary>
    /// Open the desired web URL into the default browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    public void OpenLink(string url);
    
    /// <summary>
    /// Check if the developer mode is enabled or not.
    /// </summary>
    /// <returns>True if the developer mode is enabled, false otherwise.</returns>
    public bool IsDeveloperMode();
}