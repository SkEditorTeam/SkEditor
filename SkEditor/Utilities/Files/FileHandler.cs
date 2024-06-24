using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities.Parser;
using SkEditor.Utilities.Projects;
using SkEditor.Utilities.Syntax;
using SkEditor.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace SkEditor.Utilities.Files;
public class FileHandler
{
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

    public static async void SaveFile()
    {
        if (!SkEditorAPI.Files.IsEditorOpen())
            return;

        await SkEditorAPI.Files.Save(SkEditorAPI.Files.GetCurrentOpenedFile(), false);
    }

    public static async void SaveAsFile()
    {
        if (!SkEditorAPI.Files.IsEditorOpen())
            return;

        await SkEditorAPI.Files.Save(SkEditorAPI.Files.GetCurrentOpenedFile(), true);
    }

    public static void SwitchTab(int index)
    {
        if (index < SkEditorAPI.Files.GetOpenedTabs().Count) SkEditorAPI.Files.Select(index);
    }
}