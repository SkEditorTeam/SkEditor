using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using SkEditor.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Editor;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Parser;
using SkEditor.Utilities.Syntax;
using SkEditor.ViewModels;
using SkEditor.Views.FileTypes;
using File = System.IO.File;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.API;

public class Files : IFiles
{
    private List<OpenedFile> OpenedFiles { get; } = [];

    #region Saving

    public async Task Save(object entity, bool saveAs)
    {
        TabViewItem? tabItem = GetItem(entity);
        if (tabItem == null)
        {
            return;
        }

        OpenedFile? openedFile = GetOpenedFiles().Find(file => file.TabViewItem == tabItem);
        if (openedFile == null)
        {
            return;
        }

        if (openedFile.IsSaved && !saveAs)
        {
            return;
        }

        string? path = GetFromTabViewItem(tabItem)?.Path;
        if (path == null || saveAs)
        {
            IStorageFolder? suggestedFolder;
            IStorageProvider? storageProvider = SkEditorAPI.Windows.GetMainWindow()?.StorageProvider;

            if (storageProvider == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                suggestedFolder = await storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
            }
            else
            {
                suggestedFolder = await storageProvider.TryGetFolderFromPathAsync(path);
            }

            FilePickerFileType skriptFileType = new("Skript") { Patterns = ["*.sk"] };
            FilePickerFileType allFilesType = new("All Files") { Patterns = ["*"] };

            FilePickerSaveOptions saveOptions = new()
            {
                Title = Translation.Get("WindowTitleSaveFilePicker"),
                SuggestedFileName = "",
                DefaultExtension = Path.GetExtension(path) ?? ".sk",
                FileTypeChoices = [skriptFileType, allFilesType],
                SuggestedStartLocation = suggestedFolder
            };

            IStorageFile? file = await storageProvider.SaveFilePickerAsync(saveOptions);

            if (file is null)
            {
                return;
            }

            string absolutePath = Uri.UnescapeDataString(file.Path.AbsolutePath);
            string? directory = Path.GetDirectoryName(absolutePath);
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }

            Directory.CreateDirectory(directory);
            path = absolutePath;

            openedFile.Path = path;
            openedFile.CustomName = Path.GetFileName(path);
            tabItem.Header = openedFile.Header;

            Icon.SetIcon(openedFile);
        }

        try
        {
            string? textToWrite = openedFile.Editor?.Text;
            await using StreamWriter writer = new(path, false);
            await writer.WriteAsync(textToWrite);
            writer.Close();
        }
        catch (Exception e)
        {
            await SkEditorAPI.Windows.ShowError(e.Message);
            return;
        }

        openedFile.IsSaved = true;
        openedFile.IsNewFile = false;
        SkEditorAPI.Events.FileSaved(path);
    }

    #endregion

    public void AddWelcomeTab()
    {
        SymbolIconSource icon = new()
        {
            Symbol = Symbol.Home
        };
        AddCustomTab(Translation.Get("WelcomeTabTitle"), new WelcomeTabControl(), icon: icon);
    }

    private OpenedFile? GetFromTabViewItem(TabViewItem tabViewItem)
    {
        return OpenedFiles.FirstOrDefault(source => source.TabViewItem == tabViewItem);
    }

    private TabViewItem? GetItem(object entity)
    {
        return entity switch
        {
            OpenedFile openedFile => openedFile.TabViewItem,
            TabViewItem item => item,
            string path => GetOpenedFiles()?.Find(item => item.Path == path)?.TabViewItem,
            int index => GetOpenedTabs()[index],
            _ => throw new ArgumentException("Given entity is not an OpenedFile, TabViewItem, string or int")
        };
    }

    #region Getters/Conditions

    public bool IsFileOpen()
    {
        return GetCurrentOpenedFile()?.IsCustomTab == false;
    }

    public bool IsEditorOpen()
    {
        return GetCurrentOpenedFile()?.IsEditor ?? false;
    }

    public List<OpenedFile> GetOpenedFiles()
    {
        return OpenedFiles;
    }

    public OpenedFile? GetCurrentOpenedFile()
    {
        TabViewItem? tabViewItem = GetCurrentTabViewItem();
        return tabViewItem == null ? null : GetFromTabViewItem(tabViewItem);
    }

    public TabView? GetTabView()
    {
        return SkEditorAPI.Windows.GetMainWindow()?.TabControl;
    }

    public List<TabViewItem> GetOpenedTabs()
    {
        return GetOpenedFiles()
            .Select(source => source.TabViewItem)
            .OfType<TabViewItem>()
            .ToList();
    }

    public List<OpenedFile> GetOpenedEditors()
    {
        return GetOpenedFiles().Where(source => source.IsEditor).ToList();
    }

    public TabViewItem? GetCurrentTabViewItem()
    {
        return (TabViewItem?)GetTabView()?.SelectedItem;
    }

    public OpenedFile? GetOpenedFileByPath(string path)
    {
        return GetOpenedFiles().Find(file => file.Path?.NormalizePathSeparators() == path?.NormalizePathSeparators());
    }

    #endregion

    #region Tab Manipulation

    public void AddCustomTab(object header, Control content, bool select = true, IconSource? icon = null)
    {
        TabViewItem tabItem = new()
        {
            Header = header,
            Content = content
        };

        if (icon != null)
        {
            tabItem.IconSource = icon;
        }

        GetOpenedFiles().Add(new OpenedFile
        {
            TabViewItem = tabItem
        });

        TabView? tabView = GetTabView();
        if (tabView == null)
        {
            return;
        }

        (tabView.TabItems as IList)?.Add(tabItem);
        if (select)
        {
            tabView.SelectedItem = tabItem;
        }
    }

    public async Task<OpenedFile?> AddEditorTab(string content, string? path)
    {
        int index = GetOpenedEditors().Count + 1;
        string header = Translation.Get("NewFileNameFormat").Replace("{0}", index.ToString());

        TabViewItem? tabItem = await FileBuilder.Build(header);
        if (tabItem == null)
        {
            return null;
        }

        tabItem.Tag = path;

        if (tabItem.Content is not TextEditor editor)
        {
            return null;
        }

        editor.Text = content;

        OpenedFile openedFile = new()
        {
            Editor = editor,
            Path = path,
            TabViewItem = tabItem,
            CustomName = header,
            IsSaved = !string.IsNullOrEmpty(path),
            IsNewFile = path == null
        };

        if (SkEditorAPI.Core.GetAppConfig().IsZoomSyncEnabled &&
            GetOpenedEditors()?.FirstOrDefault()?.Editor is { } firstEditor)
        {
            editor.FontSize = firstEditor.FontSize;
        }

        Icon.SetIcon(openedFile);

        // Custom Data
        openedFile["Parser"] = new CodeParser(editor);
        openedFile["Margin"] = new EditorMargin(openedFile);

        RemoveWelcomeTab();
        GetOpenedFiles().Add(openedFile);
        (GetTabView()?.TabItems as IList)?.Add(tabItem);
        await SyntaxLoader.RefreshSyntaxAsync();
        return openedFile;
    }

    public async Task NewFile(string content = "")
    {
        await AddEditorTab(content, null);
    }

    public async Task OpenFile(string path, bool force = false)
    {
        path = Uri.UnescapeDataString(Path.GetFullPath(path));
        if (!File.Exists(path))
        {
            return;
        }

        OpenedFile? alreadyOpenedFile = GetOpenedFileByPath(path);
        if (alreadyOpenedFile != null)
        {
            Select(alreadyOpenedFile);
            return;
        }

        OpenedFile? openedFile = null;

        string extension = Path.GetExtension(path);
        List<FileTypeData> availableTypes = Registries.FileTypes
            .Where(fileType => fileType.SupportedExtensions.Contains(extension))
            .ToList();

        switch (availableTypes.Count)
        {
            case 1:
                openedFile = BuildFromType(availableTypes[0], path);
                break;
            case > 1:
            {
                string? configuredTypeFullId =
                    SkEditorAPI.Core.GetAppConfig().FileTypeChoices.GetValueOrDefault(extension, null);

                if (configuredTypeFullId != null && !Registries.FileTypes.HasFullKey(configuredTypeFullId))
                {
                    configuredTypeFullId = null;
                }

                if (configuredTypeFullId != null)
                {
                    RegistryKey key = RegistryKey.FromFullKey(configuredTypeFullId);
                    var fileType = Registries.FileTypes.GetValue(key);
                    if (fileType != null)
                    {
                        openedFile = BuildFromType(fileType, path);
                    }
                }
                else
                {
                    FileTypeSelectionViewModel selectionVm = new()
                    {
                        FileTypes = availableTypes,
                        SelectedFileType = null
                    };
                    await SkEditorAPI.Windows.ShowWindowAsDialog(new FileTypeSelectionWindow
                        { DataContext = selectionVm });

                    if (selectionVm.SelectedFileType == null)
                    {
                        return;
                    }

                    if (selectionVm.RememberSelection)
                    {
                        SkEditorAPI.Core.GetAppConfig().FileTypeChoices[extension] =
                            Registries.FileTypes.GetValueKey(selectionVm.SelectedFileType)?.FullKey;
                    }

                    openedFile = BuildFromType(selectionVm.SelectedFileType, path);
                }

                break;
            }
            default:
            {
                string content = await File.ReadAllTextAsync(path);
                // binary check
                if (!force && content.Any(c => char.IsControl(c) && !char.IsWhiteSpace(c)))
                {
                    ContentDialogResult response = await SkEditorAPI.Windows.ShowDialog("BinaryFileTitle",
                        "BinaryFileFound",
                        cancelButtonText: "Cancel", icon: FluentAvalonia.UI.Controls.Symbol.Code);
                    if (response != ContentDialogResult.Primary)
                    {
                        return;
                    }
                }

                openedFile = await AddEditorTab(content, path);
                break;
            }
        }

        if (openedFile == null)
        {
            return;
        }

        Icon.SetIcon(openedFile);

        SkEditorAPI.Events.FileOpened(openedFile, false);

        RemoveWelcomeTab();
        Select(openedFile);
        return;

        OpenedFile? BuildFromType(FileTypeData fileType, string filePath)
        {
            FileTypeResult result = fileType.FileOpener(filePath) ?? new FileTypeResult(null);
            if (result.Control == null)
            {
                return null;
            }

            TabViewItem tabViewItem = new()
            {
                Header = result.Header ?? Path.GetFileName(filePath),
                IsSelected = true,
                Content = result.Control,
                Tag = string.Empty
            };

            openedFile = new OpenedFile
            {
                TabViewItem = tabViewItem,
                Path = filePath,
                IsSaved = true
            };

            GetOpenedFiles().Add(openedFile);
            (GetTabView()?.TabItems as IList)?.Add(tabViewItem);
            return openedFile;
        }
    }

    private void RemoveWelcomeTab()
    {
        OpenedFile? welcomeTab = GetOpenedFiles().Find(file => file.TabViewItem?.Content is WelcomeTabControl);
        if (welcomeTab == null)
        {
            return;
        }

        (GetTabView()?.TabItems as IList)?.Remove(welcomeTab.TabViewItem);
        GetOpenedFiles().Remove(welcomeTab);
    }

    public void Select(object entity)
    {
        if (GetTabView() is { } tabView)
        {
            tabView.SelectedItem = GetItem(entity);
        }
    }

    public async Task Close(object entity)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            TabViewItem? tabViewItem = GetItem(entity);
            if (tabViewItem == null)
            {
                return;
            }

            OpenedFile? file = GetOpenedFiles().Find(source => source.TabViewItem == tabViewItem);

            if (file == null)
            {
                return;
            }

            if (!file.IsSaved && file is { IsEditor: true })
            {
                ContentDialogResult response = await SkEditorAPI.Windows.ShowDialog(
                    Translation.Get("UnsavedFileTitle"),
                    Translation.Get("UnsavedFileMessage", file.Name),
                    primaryButtonText: Translation.Get("Yes"),
                    cancelButtonText: Translation.Get("CancelButton"), 
                    icon: FluentAvalonia.UI.Controls.Symbol.SaveLocal, translate: false);
                if (response != ContentDialogResult.Primary)
                {
                    return;
                }
            }

            bool canClose = SkEditorAPI.Events.TabClosed(file);
            if (!canClose)
            {
                return;
            }

            if (tabViewItem.Content is TextEditor editor)
            {
                TextEditorEventHandler.ScrollViewers.Remove(editor);
            }

            (GetTabView()?.TabItems as IList)?.Remove(tabViewItem);
            OpenedFiles.RemoveAll(opFile => opFile.TabViewItem == tabViewItem);

            if (OpenedFiles.Count == 0)
            {
                AddWelcomeTab();
            }
        });
    }

    public async Task BatchClose(IFiles.FileCloseAction closeAction)
    {
        List<OpenedFile> openedFiles = new(GetOpenedFiles());

        switch (closeAction)
        {
            case IFiles.FileCloseAction.AllExceptCurrent:
            {
                OpenedFile? currentOpenedFile = GetCurrentOpenedFile();
                List<OpenedFile> filesToClose =
                    openedFiles.Where(openedFile => openedFile != currentOpenedFile).ToList();

                foreach (OpenedFile file in filesToClose)
                {
                    await Close(file);
                }

                break;
            }
            case IFiles.FileCloseAction.Unsaved:
            {
                List<OpenedFile> unsavedFiles = openedFiles.Where(openedFile => !openedFile.IsSaved).ToList();

                foreach (OpenedFile file in unsavedFiles)
                {
                    await Close(file);
                }

                break;
            }
            case IFiles.FileCloseAction.AllRight:
            case IFiles.FileCloseAction.AllLeft:
            {
                TabViewItem? currentTabViewItem = GetCurrentTabViewItem();
                int? index = GetTabView()?.TabItems.IndexOf(currentTabViewItem);
                if (index is null or < 0)
                {
                    return;
                }

                List<TabViewItem> openedTabs = new(GetOpenedTabs());

                List<TabViewItem> itemsToClose = closeAction == IFiles.FileCloseAction.AllRight
                    ? openedTabs.GetRange(index.Value + 1, openedTabs.Count - index.Value - 1)
                    : openedTabs.GetRange(0, index.Value);

                itemsToClose.RemoveAll(tab => tab == currentTabViewItem);

                foreach (TabViewItem tab in itemsToClose)
                {
                    await Close(tab);
                }

                break;
            }
            case IFiles.FileCloseAction.All:
            {
                foreach (OpenedFile file in openedFiles)
                {
                    await Close(file);
                }

                if (GetOpenedFiles().Count == 0)
                {
                    AddWelcomeTab();
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(closeAction), closeAction, null);
        }
    }

    #endregion
}