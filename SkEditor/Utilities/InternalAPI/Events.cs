using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace SkEditor.API;

public class Events : IEvents
{
    
    public event EventHandler? OnPostEnable;
    public void PostEnable() => OnPostEnable?.Invoke(this, EventArgs.Empty);
    
    public event EventHandler<FileOpenedEventArgs>? OnFileOpened;
    public void FileOpened(object content, string filePath, TabViewItem tabViewItem, bool causedByRestore) => 
        OnFileOpened?.Invoke(this, new FileOpenedEventArgs(content, filePath, tabViewItem, causedByRestore));
}

public class FileOpenedEventArgs(object content, string filePath, TabViewItem tabViewItem, bool causedByRestore) : EventArgs
{
    public object Content { get; } = content;
    public string FilePath { get; } = filePath;
    public TabViewItem TabViewItem { get; } = tabViewItem;
    public bool CausedByRestore { get; set; } = causedByRestore;
}