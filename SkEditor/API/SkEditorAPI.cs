using System;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.InternalAPI.Classes;
using WindowsAPI = SkEditor.Utilities.InternalAPI.Classes.Windows;

namespace SkEditor.API;

/// <summary>
///     The main API class for SkEditor, that regroups all the API classes.
/// </summary>
public static class SkEditorAPI
{
    /// <summary>
    ///     Get the Logs API.
    /// </summary>
    [Obsolete("move to proper DI")]
    public static ILogService Logs => App.Services.GetRequiredService<ILogService>();

    /// <summary>
    ///     Get the Windows API.
    /// </summary>
    public static IWindows Windows { get; } = new WindowsAPI();

    /// <summary>
    ///     Get the Core API.
    /// </summary>
    public static ICore Core { get; } = new Core();

    /// <summary>
    ///     Get the Events API.
    /// </summary>
    public static IEvents Events { get; } = new Events();

    /// <summary>
    ///     Get the Addons API.
    /// </summary>
    public static IAddons Addons { get; } = new Addons();

    /// <summary>
    ///     Get the Files API.
    /// </summary>
    public static IFiles Files { get; } = new Files();

    /// <summary>
    ///     Get the Configs API.
    /// </summary>
    public static IConfigs Configs { get; } = new Configs();
}