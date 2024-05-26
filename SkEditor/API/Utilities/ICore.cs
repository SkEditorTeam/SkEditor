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
}