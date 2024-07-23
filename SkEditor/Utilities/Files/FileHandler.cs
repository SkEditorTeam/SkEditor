using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Utilities.Projects;
using SkEditor.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Path = System.IO.Path;

namespace SkEditor.Utilities.Files;
public class FileHandler
{
    private static readonly ConcurrentQueue<Action> SaveQueue = new();
    private static readonly AutoResetEvent SaveEvent = new(false);
    private static readonly Thread SaveThread;

    static FileHandler()
    {
        SaveThread = new Thread(ProcessSaveQueue)
        {
            IsBackground = true
        };
        SaveThread.Start();
    }

    private static void ProcessSaveQueue()
    {
        while (true)
        {
            SaveEvent.WaitOne();
            while (SaveQueue.TryDequeue(out var saveAction))
            {
                saveAction();
            }
        }
    }

    public static void QueueSave(Action saveAction)
    {
        SaveQueue.Enqueue(saveAction);
        SaveEvent.Set();
    }

    public static void SaveFile()
    {
        if (!SkEditorAPI.Files.IsEditorOpen())
            return;

        QueueSave(async () => await Dispatcher.UIThread.InvokeAsync(async () => 
            await SkEditorAPI.Files.Save(SkEditorAPI.Files.GetCurrentOpenedFile(), false)));
    }

    public static void SaveAsFile()
    {
        if (!SkEditorAPI.Files.IsEditorOpen())
            return;

        QueueSave(async () => await Dispatcher.UIThread.InvokeAsync(async () =>
            await SkEditorAPI.Files.Save(SkEditorAPI.Files.GetCurrentOpenedFile(), true)));
    }

    public static void SaveAllFiles()
    {
        var openedEditors = SkEditorAPI.Files.GetOpenedEditors();
        foreach (var file in openedEditors)
        {
            QueueSave(async () => await Dispatcher.UIThread.InvokeAsync(async () =>
                await SkEditorAPI.Files.Save(file, false)));
        }
    }

    public static Action<AppWindow, DragEventArgs> FileDropAction = (window, e) =>
    {
        try
        {
            string? folder = e.Data.GetFiles().FirstOrDefault(f => Directory.Exists(f.Path.AbsolutePath))?.Path.AbsolutePath;
            if (folder != null)
            {
                ProjectOpener.OpenProject(folder);
                return;
            }

            e.Data.GetFiles().Where(f => !Directory.Exists(f.Path.AbsolutePath)).ToList().ForEach(file =>
            {
                OpenFile(file.Path.AbsolutePath);
            });
        }
        catch { }
    };

    public static void TabSwitchAction()
    {
        if (SkEditorAPI.Files.GetOpenedTabs().Count == 0 || !SkEditorAPI.Files.IsEditorOpen())
            return;
        OpenedFile file = SkEditorAPI.Files.GetCurrentOpenedFile();

        var fileType = FileBuilder.OpenedFiles.GetValueOrDefault(file.Header.ToString());
        MainWindow.Instance.BottomBar.IsVisible = fileType?.NeedsBottomBar ?? true;
    }

    public static void NewFile()
    {
        SkEditorAPI.Files.NewFile();
    }

    public static async void OpenFile()
    {
        var files = await SkEditorAPI.Windows.GetMainWindow().StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Translation.Get("WindowTitleOpenFilePicker"),
            AllowMultiple = true
        });
      
        files.ToList().ForEach(file => OpenFile(file.Path.AbsolutePath));
    }

    public static void OpenFile(string path)
    {
        SkEditorAPI.Files.OpenFile(path);
    }

    private static void AddChangeChecker(string path, TabViewItem tabItem)
    {
        if (!SkEditorAPI.Core.GetAppConfig().CheckForChanges || tabItem.Content is not TextEditor) return;

        FileSystemWatcher watcher = new(Path.GetDirectoryName(path), Path.GetFileName(path));
        watcher.Changed += (sender, e) => ChangeChecker.HasChangedDictionary[path] = true;
        (tabItem.Content as TextEditor).Unloaded += (sender, e) =>
        {
            if (SkEditorAPI.Files.GetOpenedFiles().FirstOrDefault(openedFile => openedFile.TabViewItem == tabItem) is not OpenedFile openedFile)
            {
                watcher.Dispose();
            }
        };
    }

    public static void SwitchTab(int index)
    {
        if (index < SkEditorAPI.Files.GetOpenedTabs().Count) SkEditorAPI.Files.Select(index);
    }
}