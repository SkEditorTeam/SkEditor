using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Files;

namespace SkEditor.Utilities.Projects;
public static class ProjectOpener
{
    private static TreeView FileTreeView => ApiVault.Get().GetMainWindow().SideBar.FileTreeView;
    private static IStorageFolder ProjectRootFolder;

    public async static void OpenProject()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(ApiVault.Get().GetMainWindow());

        IReadOnlyList<IStorageFolder> folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            SuggestedStartLocation = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Desktop),
            AllowMultiple = false
        });

        if (folder.Count == 0) return;

        FileTreeView.Items.Clear();

        foreach (IStorageFolder storageFolder in folder)
        {
            ProjectRootFolder = storageFolder;
            TreeViewItem rootFolder = CreateTreeViewItem(storageFolder, true);
            rootFolder.IsExpanded = true;

            FileTreeView.Items.Add(rootFolder);

            AddChildren(rootFolder, storageFolder);
        }
    }

    public async static void AddChildren(TreeViewItem viewItem, IStorageFolder folder)
    {
        await foreach (IStorageItem storageItem in folder.GetItemsAsync())
        {
            string path = Uri.UnescapeDataString(storageItem.Path.AbsolutePath);
            if (storageItem is IStorageFolder storageFolder)
            {
                TreeViewItem folderItem = CreateTreeViewItem(storageFolder);
                folderItem.ContextFlyout = CreateContextMenu(folderItem, storageFolder);
                viewItem.Items.Add(folderItem);
                
                AddChildren(folderItem, storageFolder);
            }
            else
            {
                TreeViewItem item = CreateTreeViewItem(storageItem);

                item.DoubleTapped += (sender, e) =>
                {
                    FileHandler.OpenFile(path);
                };

                item.ContextFlyout = null;

                viewItem.Items.Add(item);
            }
        }
    }

    #region TabViewItem Creation

    public static TreeViewItem CreateTreeViewItem(IStorageItem storageItem, bool root = false)
    {
        bool isFolder = storageItem is IStorageFolder;
        TreeViewItem item = new TreeViewItem { IsExpanded = false };

        StackPanel stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
        Label label = new Label() { Content = storageItem.Name };

        Control icon = root
            ? new SymbolIcon() { Symbol = Symbol.Home, FontSize = 16 } 
            : GetFileIcon(isFolder, Path.GetExtension(storageItem.Name)) as Control;
        
        stackPanel.Children.Add(icon);
        stackPanel.Children.Add(label);
        
        item.Header = stackPanel;
        item.Tag = storageItem.Path.AbsolutePath;
        item.FontWeight = isFolder ? FontWeight.Medium : FontWeight.Normal;
        
        return item;
    }

    private const int IconSize = 18;
    private static object GetFileIcon(bool isFolder, string extension)
    {
        if (isFolder) 
            return new SymbolIcon() { Symbol = Symbol.Folder, FontSize = IconSize };

        IconSource source = Icon.GetIcon(extension);
        if (source != null) 
            return new IconSourceElement() { IconSource = source, Width = IconSize, Height = IconSize };
        
        return new SymbolIcon() { Symbol = Symbol.Document, FontSize = IconSize };
    }

    private static TreeViewItem CreateInputItem(TreeViewItem parent, 
        Action<string> onValidation)
    {
        TreeViewItem item = new TreeViewItem
        {
            IsExpanded = false,
            Header = new TextBox()
            {
                Watermark = "File/folder name ...", 
                IsReadOnly = false, 
                Width = 150
            },
            FontWeight = FontWeight.Medium
        };

        (item.Header as TextBox).LostFocus += (_, _) =>
        {
            parent.Items.Remove(item);
        };
        
        (item.Header as TextBox).KeyDown += (_, args) =>
        {
            if (args.Key == Key.Enter)
                onValidation((item.Header as TextBox).Text);
        };

        item.Focus(NavigationMethod.Pointer);
        
        return item;
    }

    #endregion

    #region ContextMenu Creation
    
    private static MenuFlyout CreateContextMenu(TreeViewItem treeViewItem, IStorageItem storageItem)
    {
        var commands = new[]
        {
            new { Header = "MenuHeaderNewFile", Command = new RelayCommand(() => CreateElement(treeViewItem, storageItem)), Icon = Symbol.New },
            new { Header = "MenuHeaderNewFolder", Command = new RelayCommand(() => CreateElement(treeViewItem, storageItem, true)), Icon = Symbol.NewFolder },
            new { Header = "MenuHeaderOpenInExplorer", Command = new RelayCommand(() => DeleteItem(storageItem)), Icon = Symbol.OpenLocal },
            null,
            new { Header = "MenuHeaderCopyPath", Command = new RelayCommand(() => CopyPath(storageItem)), Icon = Symbol.Copy },
            new { Header = "MenuHeaderCopyAbsolutePath", Command = new RelayCommand(() => CopyPath(storageItem, true)), Icon = Symbol.Copy },
            null,
            new { Header = "MenuHeaderRename", Command = new RelayCommand(() => DeleteItem(storageItem)), Icon = Symbol.Rename },
            new { Header = "MenuHeaderDelete", Command = new RelayCommand(() => DeleteItem(storageItem)), Icon = Symbol.Delete }
        };

        var contextMenu = new MenuFlyout();
        List<Control> list = new List<Control>();
        foreach (var item in commands)
        {
            if (item == null)
                list.Add(new Separator());
            else 
                list.Add(new MenuItem { Header = Translation.Get(item.Header), Command = item.Command, Icon = new SymbolIcon { Symbol = item.Icon, FontSize = 20 } });
        }

        list.ForEach(item => contextMenu.Items.Add(item));

        return contextMenu;
    }
    
    private static async void DeleteItem(IStorageItem storageItem)
    {
        if (storageItem is IStorageFolder storageFolder)
        {
            await storageFolder.DeleteAsync();
            
            var parent = FileTreeView.SelectedItem as TreeViewItem;
            parent.Items.Remove(parent);
            
            SortTabItem(parent);
        }
        else
        {
            await storageItem.DeleteAsync();
            
            var parent = FileTreeView.SelectedItem as TreeViewItem;
            parent.Items.Remove(parent);
            
            SortTabItem(parent);
        }
    }
    
    private static void CreateElement(TreeViewItem treeViewItem, 
        IStorageItem storageItem, bool isFolder = false)
    {
        if (storageItem is IStorageFolder storageFolder)
        {
            if (!treeViewItem.IsExpanded)
                treeViewItem.IsExpanded = true;
            
            
            TreeViewItem item = CreateInputItem(treeViewItem, async (name) =>
            {
                if (isFolder) 
                    await storageFolder.CreateFolderAsync(name);
                else 
                    await storageFolder.CreateFileAsync(name);

                TreeViewItem createdViewItem = CreateTreeViewItem(storageItem);
                treeViewItem.IsExpanded = true;
                
                treeViewItem.Items.Add(createdViewItem);
            });
            
            treeViewItem.Items.Insert(0, item);
            item.Focus(NavigationMethod.Pointer);
        }
    }
    
    private static async void CopyPath(IStorageItem storageItem, bool absolutePath = false)
    {
        if (absolutePath)
        {
            await  ApiVault.Get().GetMainWindow().Clipboard.SetTextAsync(storageItem.Path.AbsolutePath);
            return;
        }
        
        IStorageFolder folder = storageItem as IStorageFolder;
        while (folder != null && folder.Path != ProjectRootFolder.Path)
            folder = await folder.GetParentAsync();
        
        if (folder == null) 
            return;
        
        string path = storageItem.Path.AbsolutePath.Replace(folder.Path.AbsolutePath, "");
        await ApiVault.Get().GetMainWindow().Clipboard.SetTextAsync(path);
    }

    #endregion

    #region Sorting

    private static void SortTabItem(TreeViewItem parent)
    {
        var folders = parent.Items.OfType<TabViewItem>().Where(item => item.Tag is IStorageFolder).OrderBy(item => item.Header);
        var files = parent.Items.OfType<TabViewItem>().Where(item => item.Tag is IStorageFile).OrderBy(item => item.Header);
        
        parent.Items.Clear();
        foreach (var folder in folders) parent.Items.Add(folder);
        foreach (var file in files) parent.Items.Add(file);
    }

    #endregion
}
