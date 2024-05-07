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
    public static Regex RegexPattern => new(Translation.Get("NewFileNameFormat").Replace("{0}", @"[0-9]+"));

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


    public static List<OpenedFile> OpenedFiles { get; } = [];

    public static void TabSwitchAction()
    {
        if (ApiVault.Get().GetTabView().SelectedItem is not TabViewItem item) return;

        var fileType = FileBuilder.OpenedFiles.GetValueOrDefault(item.Header.ToString());
        MainWindow.Instance.BottomBar.IsVisible = fileType?.NeedsBottomBar ?? true;
    }


    private static int GetUntitledNumber() => (ApiVault.Get().GetTabView().TabItems as IList).Cast<TabViewItem>().Count(tab => RegexPattern.IsMatch(tab.Header.ToString())) + 1;

    public static async void NewFile()
    {
        string header = Translation.Get("NewFileNameFormat").Replace("{0}", GetUntitledNumber().ToString());
        TabViewItem tabItem = await FileBuilder.Build(header);

        if (tabItem == null) return;

        OpenedFiles.Add(new OpenedFile()
        {
            Editor = tabItem.Content as TextEditor,
            Path = "",
            TabViewItem = tabItem,
            Parser = tabItem.Content is TextEditor editor
                ? new CodeParser(editor)
                : null
        });

        (ApiVault.Get().GetTabView().TabItems as IList)?.Add(tabItem);
    }

    public async static void OpenFile()
    {
        var topLevel = TopLevel.GetTopLevel(ApiVault.Get().GetMainWindow());

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = Translation.Get("WindowTitleOpenFilePicker"),
            AllowMultiple = true
        });

        files.ToList().ForEach(file => OpenFile(file.Path.AbsolutePath));
    }

    public static async void OpenFile(string path)
    {
        bool untitledFileOpen = IsOnlyEmptyFileOpen();

        path = Uri.UnescapeDataString(Path.GetFullPath(path));
        if (CheckAlreadyOpen(path)) return;

        string fileName = Path.GetFileName(path);
        TabViewItem tabItem = await FileBuilder.Build(fileName, path);
        if (tabItem == null) return;

        (ApiVault.Get().GetTabView().TabItems as IList)?.Add(tabItem);
        if (untitledFileOpen) await FileCloser.CloseFile((ApiVault.Get().GetTabView().TabItems as IList)[0] as TabViewItem);

        OpenedFiles.Add(new OpenedFile()
        {
            Editor = tabItem.Content as TextEditor,
            Path = path,
            TabViewItem = tabItem,
            Parser = tabItem.Content is TextEditor editor
                ? new CodeParser(editor)
                : null
        });

        AddChangeChecker(path, tabItem);
    }

    private static void AddChangeChecker(string path, TabViewItem tabItem)
    {
        if (!ApiVault.Get().GetAppConfig().CheckForChanges || tabItem.Content is not TextEditor) return;

        FileSystemWatcher watcher = new(Path.GetDirectoryName(path), Path.GetFileName(path));
        watcher.Changed += (sender, e) => ChangeChecker.HasChangedDictionary[path] = true;
        (tabItem.Content as TextEditor).Unloaded += (sender, e) =>
        {
            if (OpenedFiles.FirstOrDefault(openedFile => openedFile.TabViewItem == tabItem) is not OpenedFile openedFile)
            {
                watcher.Dispose();
            }
        };
    }

    private static bool CheckAlreadyOpen(string path)
    {
        if ((ApiVault.Get().GetTabView().TabItems as IList).Cast<TabViewItem>().Where(tab => tab.Tag != null).Any(tab => tab.Tag.ToString() == path))
        {
            ApiVault.Get().GetTabView().SelectedItem = (ApiVault.Get().GetTabView().TabItems as IList)
                .Cast<TabViewItem>()
                .First(tab => tab.Tag.ToString() == path);
            return true;
        }
        return false;
    }

    private static bool IsOnlyEmptyFileOpen()
    {
        return ApiVault.Get().GetTabView().TabItems.Count() == 1 &&
            ApiVault.Get().GetTextEditor() != null &&
            ApiVault.Get().GetTextEditor().Text.Length == 0 &&
            ApiVault.Get().GetTabView().SelectedItem is TabViewItem item &&
            item.Header.ToString().Contains(Translation.Get("NewFileNameFormat").Replace("{0}", "")) &&
            !item.Header.ToString().EndsWith('*');
    }

    public static async Task<(bool, Exception)> SaveFile()
    {
        if (!ApiVault.Get().IsFileOpen()) return (true, null);

        try
        {
            TabViewItem item = ApiVault.Get().GetTabView().SelectedItem as TabViewItem;
            string path = item.Tag.ToString();

            if (string.IsNullOrEmpty(path))
            {
                SaveAsFile();
                return (true, null);
            }

            string textToWrite = ApiVault.Get().GetTextEditor().Text;
            using StreamWriter writer = new(path, false);
            await writer.WriteAsync(textToWrite);

            if (!item.Header.ToString().EndsWith('*')) return (true, null);
            item.Header = item.Header.ToString()[..^1];
        }
        catch (Exception e)
        {
            Log.Warning(e, "Failed to save file");
            return (false, e);
        }
        return (true, null);
    }

    public async static void SaveAsFile()
    {
        if (!ApiVault.Get().IsFileOpen()) return;

        var topLevel = TopLevel.GetTopLevel(ApiVault.Get().GetMainWindow());
        var tabView = ApiVault.Get().GetTabView();

        if (tabView.SelectedItem is not TabViewItem item) return;

        string header = item.Header.ToString().TrimEnd('*');
        string itemTag = item.Tag.ToString();
        IStorageFolder suggestedFolder = string.IsNullOrEmpty(itemTag)
            ? await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
            : await topLevel.StorageProvider.TryGetFolderFromPathAsync(itemTag);

        FilePickerFileType skriptFileType = new("Skript") { Patterns = ["*.sk"] };
        FilePickerFileType allFilesType = new("All Files") { Patterns = ["*"] };

        FilePickerSaveOptions saveOptions = new()
        {
            Title = Translation.Get("WindowTitleSaveFilePicker"),
            SuggestedFileName = header,
            DefaultExtension = Path.GetExtension(itemTag) ?? ".sk",
            FileTypeChoices = [skriptFileType, allFilesType],
            SuggestedStartLocation = suggestedFolder
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(saveOptions);

        if (file is null) return;

        string absolutePath = Uri.UnescapeDataString(file.Path.AbsolutePath);

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
        using var stream = File.OpenWrite(absolutePath);
        ApiVault.Get().GetTextEditor().Save(stream);

        item.Header = file.Name;
        item.Tag = Uri.UnescapeDataString(absolutePath);

        Icon.SetIcon(item);
        ToolTip toolTip = new()
        {
            Content = absolutePath,
        };
        ToolTip.SetTip(item, toolTip);

        AddChangeChecker(absolutePath, item);
    }

    public static void SwitchTab(int index)
    {
        var tabView = ApiVault.Get().GetTabView();
        if (index < tabView.TabItems.Count()) tabView.SelectedIndex = index;
    }
}
