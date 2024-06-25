using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using SkEditor.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Editor;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Utilities.Parser;
using SkEditor.Utilities.Syntax;
using File = System.IO.File;

namespace SkEditor.API;

public class Files : IFiles
{
    private List<OpenedFile> OpenedFiles { get; } = [];

    #region Getters/Conditions

    public bool IsFileOpen()
    {
        return !GetCurrentOpenedFile().IsCustomTab;
    }

    public bool IsEditorOpen()
    {
        return GetCurrentOpenedFile()?.IsEditor ?? false;
    }

    public List<OpenedFile> GetOpenedFiles()
    {
        return OpenedFiles;
    }

    public OpenedFile GetCurrentOpenedFile()
    {
        return GetFromTabViewItem(GetCurrentTabViewItem());
    }

    public TabView GetTabView()
    {
        return SkEditorAPI.Windows.GetMainWindow().TabControl;
    }

    public List<TabViewItem> GetOpenedTabs()
    {
        return GetOpenedFiles().Select(source => source.TabViewItem).ToList();
    }

    public List<OpenedFile> GetOpenedEditors()
    {
        return GetOpenedFiles().Where(source => source.IsEditor).ToList();
    }
    
    public TabViewItem GetCurrentTabViewItem()
    {
        return (TabViewItem) GetTabView().SelectedItem;
    }

    public OpenedFile? GetOpenedFileByPath(string path)
    {
        return GetOpenedFiles().Find(file => file.Path == path);
    }

    #endregion

    #region Saving
    
    public async Task Save(object entity, bool saveAs)
    {
        var tabItem = GetItem(entity);
        var itemTag = tabItem.Tag as string;
        var openedFile = GetOpenedFiles().Find(file => file.TabViewItem == tabItem);

        if (openedFile.IsSaved)
            return;

        var path = openedFile.Path == null ? null : Uri.UnescapeDataString(openedFile.Path);
        if (path == null || saveAs)
        {
            var suggestedFolder = string.IsNullOrEmpty(itemTag)
                ? await SkEditorAPI.Windows.GetMainWindow().StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents)
                : await SkEditorAPI.Windows.GetMainWindow().StorageProvider.TryGetFolderFromPathAsync(itemTag);

            FilePickerFileType skriptFileType = new("Skript") { Patterns = ["*.sk"] };
            FilePickerFileType allFilesType = new("All Files") { Patterns = ["*"] };

            FilePickerSaveOptions saveOptions = new()
            {
                Title = Translation.Get("WindowTitleSaveFilePicker"),
                SuggestedFileName = "",
                DefaultExtension = Path.GetExtension(itemTag) ?? ".sk",
                FileTypeChoices = [skriptFileType, allFilesType],
                SuggestedStartLocation = suggestedFolder
            };
            
            var file = await SkEditorAPI.Windows.GetMainWindow().StorageProvider.SaveFilePickerAsync(saveOptions);
            if (file is null)
                return;
            
            var absolutePath = Uri.UnescapeDataString(file.Path.AbsolutePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
            path = absolutePath;
        }

        try
        {
            var textToWrite = openedFile.Editor?.Text;
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
        openedFile.Path = path;
    }

    #endregion

    #region Tab Manipulation

    public void AddCustomTab(object header, Control content, bool select = true)
    {
        var tabItem = new TabViewItem()
        {
            Header = header,
            Content = content
        };
        
        GetOpenedFiles().Add(new OpenedFile()
        {
            TabViewItem = tabItem,
            IsCustomTab = true
        });
        (GetTabView().TabItems as IList)?.Add(tabItem);
        if (select)
            GetTabView().SelectedItem = tabItem;
    }
    
    public async Task<OpenedFile> AddEditorTab(string content, string? path)
    {
        var header = Translation.Get("NewFileNameFormat").Replace("{0}",
            GetOpenedFiles().Count.ToString());
        
        var tabItem = await FileBuilder.Build(header);
        tabItem.Tag = path;
        (tabItem.Content as TextEditor).Text = content;

        var openedFile = new OpenedFile()
        {
            Editor = tabItem.Content as TextEditor,
            IsCustomTab = false,
            Path = path,
            TabViewItem = tabItem,
            CustomName = header,
            IsSaved = path != null,
            IsNewFile = path == null,
        };
        
        openedFile["Margin"] = new EditorMargin(openedFile);
        if (tabItem.Content is TextEditor textEditor)
            openedFile["Parser"] = new FileParser(textEditor, openedFile);

        GetOpenedFiles().Add(openedFile);
        (GetTabView().TabItems as IList)?.Add(tabItem);
        await SyntaxLoader.RefreshSyntaxAsync();
        return openedFile;
    }

    public async void NewFile(string content = "")
    {
        await AddEditorTab(content, null);
    }

    public async void OpenFile(string path)
    {
        path = Uri.UnescapeDataString(Path.GetFullPath(path));
        if (!File.Exists(path))
            return;

        var alreadyOpenedFile = GetOpenedFileByPath(path);
        if (alreadyOpenedFile != null)
        {
            Select(alreadyOpenedFile);
            return;
        }

        var content = await File.ReadAllTextAsync(path);
        var openedFile = await AddEditorTab(content, path);
        (SkEditorAPI.Events as Events).FileOpened(openedFile.TabViewItem.Content, path, 
            openedFile.TabViewItem, false);
        Select(openedFile);
    }

    public void Select(object entity)
    {
        GetTabView().SelectedItem = GetItem(entity);
    }

    public async Task Close(object entity)
    {
        var tabViewItem = GetItem(entity);
        var file = GetOpenedFiles().Find(source => source.TabViewItem == tabViewItem);

        if (!file.IsSaved && file is { IsEditor: true, IsNewFile: false })
        {
            var response = await SkEditorAPI.Windows.ShowDialog(
                "Unsaved File",
                "The file '" + file.Name +
                "' is not saved.\n\nAre you sure you want to close it and discard your changes?",
                cancelButtonText: "Cancel", icon: Symbol.SaveLocal);
            if (response != ContentDialogResult.Primary)
                return;
        }
        
        var canClose = (SkEditorAPI.Events as Events).TabClosed(file);
        if (!canClose)
            return;

        if (tabViewItem.Content is TextEditor editor)
            TextEditorEventHandler.ScrollViewers.Remove(editor);
        
        (GetTabView().TabItems as IList).Remove(tabViewItem);
        OpenedFiles.RemoveAll(opFile => opFile.TabViewItem == tabViewItem);
        
        if (OpenedFiles.Count == 0)
            AddWelcomeTab();
    }

    public async void BatchClose(IFiles.FileCloseAction closeAction)
    {
        switch (closeAction)
        {
            case IFiles.FileCloseAction.AllExceptCurrent:
            {
                var currentOpenedFile = GetCurrentOpenedFile();
                foreach (var openedFile in OpenedFiles.Where(openedFile => openedFile != currentOpenedFile))
                    await Close(openedFile);
                break;
            }
            case IFiles.FileCloseAction.Unsaved:
            {
                foreach (var tabViewItem in GetOpenedTabs().Where(tabItem => tabItem.Header.ToString().EndsWith('*')))
                    await Close(tabViewItem);
                break;
            }
            case IFiles.FileCloseAction.AllRight:
            case IFiles.FileCloseAction.AllLeft:
            {
                var currentTabViewItem = GetCurrentTabViewItem();
                var index = GetTabView().TabItems.IndexOf(currentTabViewItem);
                var openedTabs = GetOpenedTabs();

                var items = closeAction == IFiles.FileCloseAction.AllRight 
                    ? openedTabs.GetRange(index + 1, openedTabs.Count - index - 1) 
                    : openedTabs.GetRange(0, index);

                items.RemoveAll(tab => tab == currentTabViewItem);
                items.ForEach(async tab => await Close(tab));
                
                break;
            }
            case IFiles.FileCloseAction.All:
            {
                GetOpenedFiles().ForEach(async tab => await Close(tab));
                AddWelcomeTab();
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(closeAction), closeAction, null);
        }
    }

    #endregion

    public void AddWelcomeTab()
    {
        AddCustomTab("Welcome", new WelcomeTabControl());
    }

    private OpenedFile GetFromTabViewItem(TabViewItem tabViewItem)
    {
        return OpenedFiles.FirstOrDefault(source => source.TabViewItem == tabViewItem);
    }

    private TabViewItem GetItem(object entity)
    {
        return entity switch
        {
            OpenedFile openedFile => openedFile.TabViewItem,
            TabViewItem item => item,
            string path => GetOpenedFiles().Find(item => item.Path == path).TabViewItem,
            int index => GetOpenedTabs()[index],
            _ => throw new ArgumentException("Given entity is not an OpenedFile, TabViewItem, string or int")
        };
    }
}