using System;
using FluentAvalonia.UI.Controls;

namespace SkEditor.API;

/// <summary>
///     Represent datas about a possible third-party connection, displayed
///     in the connection manager.
///     This will automatically handle saving, with the provided option Key, into
///     SkEditor's settings.
/// </summary>
/// <param name="Name">The name of the connection.</param>
/// <param name="Description">A description of the connection.</param>
/// <param name="GetApiKey">A function to get the API key for the connection.</param>
/// <param name="SetApiKey">An action to set the API key for the connection.</param>
/// <param name="IconSource">The icon source for the connection. Can be null.</param>
/// <param name="DashboardUrl">The URL of the dashboard for the connection. Can be null.</param>
public record ConnectionData(
    string Name,
    string Description,
    Func<string> GetApiKey,
    Action<string> SetApiKey,
    IconSource? IconSource,
    string? DashboardUrl);