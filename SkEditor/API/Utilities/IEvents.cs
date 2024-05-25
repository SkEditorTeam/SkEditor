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
    
    // --------------------------- Editors
    
    public event EventHandler<FileOpenedEventArgs> OnFileOpened;
    
}