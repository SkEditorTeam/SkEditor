using System;

namespace SkEditor.API;

/// <summary>
/// Class handling all events in the application, that addons
/// can subscribe to.
/// </summary>
public interface IEvents
{

    /// <summary>
    /// Called when every addons has been enabled, and the
    /// first lifecycle event has been called. This is when you
    /// can do UI-related things for instance.
    /// </summary>
    public event EventHandler OnPostEnable;

    #region Editors

    public event EventHandler<FileCreatedEventArgs> OnFileCreated;

    public event EventHandler<FileOpenedEventArgs> OnFileOpened;

    /// <summary>
    /// Called when a tab view item is closed. You can
    /// cancel the close if needed.
    /// </summary>
    public event EventHandler<TabClosedEventArgs> OnTabClosed;

    #endregion

    #region Settings

    /// <summary>
    /// Called when a settings window is open.
    /// </summary>
    public event EventHandler OnSettingsOpened;

    #endregion
}