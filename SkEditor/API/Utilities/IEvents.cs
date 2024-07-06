using AvaloniaEdit;
using SkEditor.API.Settings;
using SkEditor.Utilities.Files;
using System;

namespace SkEditor.API;

/// <summary>
/// Interface handling all events in the application, that addons
/// can subscribe to.
/// </summary>
public interface IEvents
{
    /// <summary>
    /// Called when every addon has been enabled, and the
    /// first lifecycle event has been called. This is when you
    /// can do UI-related things for instance.
    /// </summary>
    event EventHandler OnPostEnable;
    internal void PostEnable();

    event EventHandler<FileCreatedEventArgs> OnFileCreated;
    internal void FileCreated(TextEditor editor);

    event EventHandler<FileOpenedEventArgs> OnFileOpened;
    internal void FileOpened(OpenedFile openedFile, bool causedByRestore);

    event EventHandler<AddonSettingChangedEventArgs> OnAddonSettingChanged;
    internal void AddonSettingChanged(Setting setting, object oldValue);

    /// <summary>
    /// Called when a tab view item is closed. You can
    /// cancel the close if needed.
    /// </summary>
    event EventHandler<TabClosedEventArgs> OnTabClosed;
    internal bool TabClosed(OpenedFile openedFile);

    /// <summary>
    /// Called when a settings window is open.
    /// </summary>
    event EventHandler OnSettingsOpened;
    internal void SettingsOpened();
}

public class FileCreatedEventArgs : EventArgs
{
    public TextEditor Editor { get; }
    public FileCreatedEventArgs(TextEditor editor) => Editor = editor;
}

public class FileOpenedEventArgs : EventArgs
{
    public bool CausedByRestore { get; set; }
    public OpenedFile OpenedFile { get; set; }
    public FileOpenedEventArgs(OpenedFile openedFile, bool causedByRestore)
    {
        OpenedFile = openedFile;
        CausedByRestore = causedByRestore;
    }
}

public class AddonSettingChangedEventArgs : EventArgs
{
    public Setting Setting { get; }
    public object OldValue { get; }
    public AddonSettingChangedEventArgs(Setting setting, object oldValue)
    {
        Setting = setting;
        OldValue = oldValue;
    }
}

public class TabClosedEventArgs : EventArgs
{
    public OpenedFile OpenedFile { get; }
    public bool CanClose { get; set; } = true;
    public TabClosedEventArgs(OpenedFile closedFile) => OpenedFile = closedFile;
}