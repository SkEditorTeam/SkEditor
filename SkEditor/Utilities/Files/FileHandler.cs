using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Utilities.Projects;
using SkEditor.Views;

namespace SkEditor.Utilities.Files;

public class FileHandler
{
    private static readonly ConcurrentQueue<Func<Task>> SaveQueue = new();
    private static readonly SemaphoreSlim SaveSemaphore = new(1, 1);

    public static readonly Func<AppWindow, DragEventArgs, Task> FileDropAction = async (_, e) =>
    {
        try
        {
            string? folder = e.Data.GetFiles()?.FirstOrDefault(f => Directory.Exists(f.Path.AbsolutePath))?.Path
                .AbsolutePath;
            if (folder != null)
            {
                await ProjectOpener.OpenProject(folder);
                return;
            }

            e.Data.GetFiles()?.Where(f => !Directory.Exists(f.Path.AbsolutePath)).ToList().ForEach(file =>
            {
                OpenFile(file.Path.AbsolutePath);
            });
        }
        catch
        {
            // ignored
        }
    };

    static FileHandler()
    {
        _ = ProcessSaveQueueAsync();
    }

    private static async Task ProcessSaveQueueAsync()
    {
        while (true)
        {
            if (SaveQueue.TryDequeue(out Func<Task>? saveAction))
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
        if (!SkEditorAPI.Files.IsEditorOpen()) return;

        QueueSave(async () => await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            OpenedFile? file = SkEditorAPI.Files.GetCurrentOpenedFile();
            if (file == null) return;
            await SkEditorAPI.Files.Save(file);
        }));
    }

    public static void SaveAsFile()
    {
        if (!SkEditorAPI.Files.IsEditorOpen()) return;

        QueueSave(async () => await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            OpenedFile? file = SkEditorAPI.Files.GetCurrentOpenedFile();
            if (file == null) return;
            await SkEditorAPI.Files.Save(file, true);
        }));
    }

    public static void SaveAllFiles()
    {
        List<OpenedFile> openedEditors = SkEditorAPI.Files.GetOpenedEditors();
        foreach (OpenedFile file in openedEditors)
        {
            QueueSave(async () => await Dispatcher.UIThread.InvokeAsync(async () =>
                await SkEditorAPI.Files.Save(file)));
        }
    }

    public static void TabSwitchAction()
    {
        if (SkEditorAPI.Files.GetOpenedTabs().Count == 0 || !SkEditorAPI.Files.IsEditorOpen())
        {
            return;
        }

        OpenedFile? file = SkEditorAPI.Files.GetCurrentOpenedFile();
        if (file == null)
        {
            MainWindow.Instance.BottomBar.IsVisible = false;
            return;
        }

        FileTypes.FileType? fileType = FileBuilder.OpenedFiles.GetValueOrDefault(file.Header);
        MainWindow.Instance.BottomBar.IsVisible = fileType?.NeedsBottomBar ?? true;
    }

    public static void NewFile()
    {
        SkEditorAPI.Files.NewFile();
    }

    public static async Task OpenFile()
    {
        MainWindow? mainWindow = SkEditorAPI.Windows.GetMainWindow();
        if (mainWindow == null) return;
        
        IReadOnlyList<IStorageFile> files = await mainWindow.StorageProvider
            .OpenFilePickerAsync(
                new FilePickerOpenOptions
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
        if (index < SkEditorAPI.Files.GetOpenedTabs().Count)
        {
            SkEditorAPI.Files.Select(index);
        }
    }
}