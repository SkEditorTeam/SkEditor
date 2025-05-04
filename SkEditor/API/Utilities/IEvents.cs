using System;
using Avalonia.Controls;
using AvaloniaEdit;
using SkEditor.API.Settings;
using SkEditor.Utilities.Files;

namespace SkEditor.API;

/// <summary>
///     Interface handling all events in the application, that addons
///     can subscribe to.
/// </summary>
public interface IEvents
{
    /// <summary>
    ///     Called when every addon has been enabled, and the
    ///     first lifecycle event has been called. This is when you
    ///     can do UI-related things for instance.
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
    ///     Called when a tab view item is closed. You can
    ///     cancel the close if needed.
    /// </summary>
    event EventHandler<TabClosedEventArgs> OnTabClosed;

    internal bool TabClosed(OpenedFile openedFile);

    event EventHandler<SelectionChangedEventArgs> OnTabChanged;
    internal void TabChanged(SelectionChangedEventArgs e);

    /// <summary>
    ///     Called when a settings window is open.
    /// </summary>
    event EventHandler OnSettingsOpened;

    internal void SettingsOpened();
}

public class FileCreatedEventArgs(TextEditor editor) : EventArgs
{
    public TextEditor Editor { get; } = editor;
}

public class FileOpenedEventArgs(OpenedFile openedFile, bool causedByRestore) : EventArgs
{
    public bool CausedByRestore { get; set; } = causedByRestore;
    public OpenedFile OpenedFile { get; set; } = openedFile;
}

public class AddonSettingChangedEventArgs(Setting setting, object oldValue) : EventArgs
{
    public Setting Setting { get; } = setting;
    public object OldValue { get; } = oldValue;
}

public class TabClosedEventArgs(OpenedFile closedFile) : EventArgs
{
    public OpenedFile OpenedFile { get; } = closedFile;
    public bool CanClose { get; set; } = true;
}