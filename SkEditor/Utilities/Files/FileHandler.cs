using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
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
using System.Threading.Tasks;

namespace SkEditor.Utilities.Files;
public class FileHandler
{
    private static readonly ConcurrentQueue<Func<Task>> SaveQueue = new();
    private static readonly SemaphoreSlim SaveSemaphore = new(1, 1);

    static FileHandler()
    {
        _ = ProcessSaveQueueAsync();
    }

    private static async Task ProcessSaveQueueAsync()
    {
        while (true)
        {
            if (SaveQueue.TryDequeue(out var saveAction))
            {
                await SaveSemaphore.WaitAsync();
                try
                {
                    await saveAction();
                }
                finally
                {
                    SaveSemaphore.Release();
                }
            }
            else
            {
                await Task.Delay(100);
            }
        }
    }

    public static void QueueSave(Func<Task> saveAction)
    {
        SaveQueue.Enqueue(saveAction);
    }

    public static void SaveFile()
    {
        if (!SkEditorAPI.Files.IsEditorOpen())
            return;

        QueueSave(async () => await Dispatcher.UIThread.InvokeAsync(async () =>
            await SkEditorAPI.Files.Save(SkEditorAPI.Files.GetCurrentOpenedFile())));
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
                await SkEditorAPI.Files.Save(file)));
        }
    }

    public static readonly Action<AppWindow, DragEventArgs> FileDropAction = (_, e) =>
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
        catch
        {
            // ignored
        }
    };

    public static void TabSwitchAction()
    {
        if (SkEditorAPI.Files.GetOpenedTabs().Count == 0 || !SkEditorAPI.Files.IsEditorOpen())
            return;
        OpenedFile file = SkEditorAPI.Files.GetCurrentOpenedFile();

        var fileType = FileBuilder.OpenedFiles.GetValueOrDefault(file.Header);
        MainWindow.Instance.BottomBar.IsVisible = fileType?.NeedsBottomBar ?? true;
    }

    public static void NewFile()
    {
        SkEditorAPI.Files.NewFile();
    }

    public static async Task OpenFile()
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

    public static void SwitchTab(int index)
    {
        if (index < SkEditorAPI.Files.GetOpenedTabs().Count) SkEditorAPI.Files.Select(index);
    }
}