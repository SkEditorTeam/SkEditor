using AvaloniaEdit;
using SkEditor.API.Settings;
using SkEditor.Utilities.Files;
using System;

namespace SkEditor.API;

public class Events : IEvents
{
    public event EventHandler? OnPostEnable;
    void IEvents.PostEnable() => OnPostEnable?.Invoke(this, EventArgs.Empty);

    public event EventHandler<FileCreatedEventArgs>? OnFileCreated;
    void IEvents.FileCreated(TextEditor editor) => OnFileCreated?.Invoke(this, new FileCreatedEventArgs(editor));

    public event EventHandler<FileOpenedEventArgs>? OnFileOpened;
    void IEvents.FileOpened(OpenedFile openedFile, bool causedByRestore)
    {
        OnFileOpened?.Invoke(this, new FileOpenedEventArgs(openedFile, causedByRestore));
    }

    public event EventHandler<AddonSettingChangedEventArgs>? OnAddonSettingChanged;
    void IEvents.AddonSettingChanged(Setting setting, object oldValue)
    {
        OnAddonSettingChanged?.Invoke(this, new AddonSettingChangedEventArgs(setting, oldValue));
    }

    public event EventHandler<TabClosedEventArgs>? OnTabClosed;
    bool IEvents.TabClosed(OpenedFile openedFile)
    {
        var args = new TabClosedEventArgs(openedFile);
        OnTabClosed?.Invoke(this, args);
        return args.CanClose;
    }

    public event EventHandler? OnSettingsOpened;
    void IEvents.SettingsOpened() => OnSettingsOpened?.Invoke(this, EventArgs.Empty);
}