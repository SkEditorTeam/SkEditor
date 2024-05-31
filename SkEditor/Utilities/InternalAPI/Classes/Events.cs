using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SkEditor.API.Settings;
using SkEditor.Utilities.Files;

namespace SkEditor.API;

public class Events : IEvents
{
    
    public event EventHandler? OnPostEnable;
    public void PostEnable() => OnPostEnable?.Invoke(this, EventArgs.Empty);
    
    public event EventHandler<FileOpenedEventArgs>? OnFileOpened;
    public void FileOpened(object content, string filePath, TabViewItem tabViewItem, bool causedByRestore) => 
        OnFileOpened?.Invoke(this, new FileOpenedEventArgs(content, filePath, tabViewItem, causedByRestore));
    
    public event EventHandler<AddonSettingChangedEventArgs>? OnAddonSettingChanged;
    public void AddonSettingChanged(Setting setting, object oldValue) => 
        OnAddonSettingChanged?.Invoke(this, new AddonSettingChangedEventArgs(setting, oldValue));
    
    public event EventHandler<TabClosedEventArgs>? OnTabClosed;
    public bool TabClosed(OpenedFile openedFile)
    {
        var args = new TabClosedEventArgs(openedFile);
        OnTabClosed?.Invoke(this, args);
        return args.CanClose;
    }
}

public class FileOpenedEventArgs(object content, string filePath, TabViewItem tabViewItem, bool causedByRestore) : EventArgs
{
    public object Content { get; } = content;
    public string FilePath { get; } = filePath;
    public TabViewItem TabViewItem { get; } = tabViewItem;
    public bool CausedByRestore { get; set; } = causedByRestore;
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