using System;
using Avalonia.Controls;
using AvaloniaEdit;
using SkEditor.API.Settings;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Projects.Elements;

namespace SkEditor.API;

public class Events : IEvents
{
    public event EventHandler? OnPostEnable;

    void IEvents.PostEnable()
    {
        OnPostEnable?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<FileCreatedEventArgs>? OnFileCreated;

    void IEvents.FileCreated(TextEditor editor)
    {
        OnFileCreated?.Invoke(this, new FileCreatedEventArgs(editor));
    }

    public event EventHandler<FileOpenedEventArgs>? OnFileOpened;

    void IEvents.FileOpened(OpenedFile openedFile, bool causedByRestore)
    {
        OnFileOpened?.Invoke(this, new FileOpenedEventArgs(openedFile, causedByRestore));
    }

    public event EventHandler<FileSavedEventArgs>? OnFileSaved;

    void IEvents.FileSaved(string path)
    {
        OnFileSaved?.Invoke(this, new FileSavedEventArgs(path));
    }

    public event EventHandler<ProjectOpenedEventArgs>? OnProjectOpened;

    void IEvents.ProjectOpened(Folder openedFolder)
    {
        OnProjectOpened?.Invoke(this, new ProjectOpenedEventArgs(openedFolder));
    }

    public event EventHandler? OnProjectClosed;

    void IEvents.ProjectClosed()
    {
        OnProjectClosed?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<AddonSettingChangedEventArgs>? OnAddonSettingChanged;

    void IEvents.AddonSettingChanged(Setting setting, object oldValue)
    {
        OnAddonSettingChanged?.Invoke(this, new AddonSettingChangedEventArgs(setting, oldValue));
    }

    public event EventHandler<TabClosedEventArgs>? OnTabClosed;

    bool IEvents.TabClosed(OpenedFile openedFile)
    {
        TabClosedEventArgs args = new(openedFile);
        OnTabClosed?.Invoke(this, args);
        return args.CanClose;
    }

    public event EventHandler<SelectionChangedEventArgs>? OnTabChanged;

    void IEvents.TabChanged(SelectionChangedEventArgs e)
    {
        OnTabChanged?.Invoke(this, e);
    }

    public event EventHandler? OnSettingsOpened;

    void IEvents.SettingsOpened()
    {
        OnSettingsOpened?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<LanguageChangedEventArgs>? OnLanguageChanged;

    void IEvents.LanguageChanged(string language)
    {
        OnLanguageChanged?.Invoke(this, new LanguageChangedEventArgs(language));
    }
}