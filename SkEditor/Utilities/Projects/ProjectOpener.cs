using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using SkEditor.Controls.Sidebar;
using SkEditor.Utilities.Projects.Elements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace SkEditor.Utilities.Projects;
public static class ProjectOpener
{
    public static Folder? ProjectRootFolder = null;
    private static ExplorerSidebarPanel Panel => ApiVault.Get().GetMainWindow().SideBar.ProjectPanel.Panel;
    public static TreeView FileTreeView => Panel.FileTreeView;
    private static StackPanel NoFolderMessage => Panel.NoFolderMessage;

    public static async void OpenProject()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(ApiVault.Get().GetMainWindow());

        IReadOnlyList<IStorageFolder> folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false
        });

        if (folders.Count == 0)
        {
            NoFolderMessage.IsVisible = ProjectRootFolder == null;
            return;
        }

        NoFolderMessage.IsVisible = false;

        ProjectRootFolder = new Folder(folders[0].Path.AbsolutePath) { IsExpanded = true };
        FileTreeView.ItemsSource = new ObservableCollection<StorageElement> { ProjectRootFolder };

        FileTreeView.DoubleTapped += (sender, e) =>
        {
            if (e.Source is not Border border) return;
            var treeViewItem = border.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
            if (treeViewItem is null) return;
            var storageElement = treeViewItem.DataContext as StorageElement;
            storageElement?.HandleDoubleClick();
        };
    }

    #region ContextMenu Creation

    private static MenuFlyout CreateContextMenu(TreeViewItem treeViewItem, IStorageItem storageItem)
    {
        var commands = new[]
        {
            new { Header = "MenuHeaderNewFile", Command = new RelayCommand(() => CreateElement(treeViewItem, storageItem)), Icon = Symbol.New },
            new { Header = "MenuHeaderNewFolder", Command = new RelayCommand(() => CreateElement(treeViewItem, storageItem, true)), Icon = Symbol.NewFolder },
            new { Header = "MenuHeaderOpenInExplorer", Command = new RelayCommand(() => OpenInExplorer(storageItem)), Icon = Symbol.OpenLocal },
            null,
            new { Header = "MenuHeaderCopyPath", Command = new RelayCommand(() => CopyPath(storageItem)), Icon = Symbol.Copy },
            new { Header = "MenuHeaderCopyAbsolutePath", Command = new RelayCommand(() => CopyPath(storageItem, true)), Icon = Symbol.Copy },
            null,
            new { Header = "MenuHeaderRename", Command = new RelayCommand(() => DeleteItem(storageItem)), Icon = Symbol.Rename },
            new { Header = "MenuHeaderDelete", Command = new RelayCommand(() => DeleteItem(storageItem)), Icon = Symbol.Delete }
        };

        var contextMenu = new MenuFlyout();

        List<Control> list = commands
            .Select(item => item == null
                ? (Control)new Separator()
                : new MenuItem { Header = Translation.Get(item.Header), Command = item.Command, Icon = new SymbolIcon { Symbol = item.Icon, FontSize = 20 } })
            .ToList();

        list.ForEach(item => contextMenu.Items.Add(item));

        return contextMenu;
    }

    private static async void DeleteItem(IStorageItem storageItem)
    {
        await storageItem.DeleteAsync();

        var parent = FileTreeView.SelectedItem as TreeViewItem;
        parent.Items.Remove(parent);

        SortTabItem(parent);
    }

    private static void CreateElement(TreeViewItem treeViewItem,
        IStorageItem storageItem, bool isFolder = false)
    {
        if (storageItem is IStorageFolder storageFolder)
        {
            treeViewItem.IsExpanded = true;

            /*
            TreeViewItem item = CreateInputItem(treeViewItem, async (name) =>
            {
                await Task.Delay(10).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(() => treeViewItem.IsExpanded = true));

                IStorageItem newStorageItem;
                if (isFolder) 
                    newStorageItem = await storageFolder.CreateFolderAsync(name);
                else 
                    newStorageItem = await storageFolder.CreateFileAsync(name);

                TreeViewItem createdViewItem = CreateTreeViewItem(newStorageItem);
                treeViewItem.IsExpanded = true;
                treeViewItem.Items.Add(createdViewItem);
                
                if (newStorageItem is IStorageFile)
                    FileHandler.OpenFile(newStorageItem.Path.AbsolutePath);
            });
            
            treeViewItem.Items.Insert(0, item);
            item.Focus(NavigationMethod.Pointer); */
        }
    }

    private static async void CopyPath(IStorageItem storageItem, bool absolutePath = false)
    {
        if (absolutePath)
        {
            await ApiVault.Get().GetMainWindow().Clipboard.SetTextAsync(storageItem.Path.AbsolutePath);
            return;
        }

        IStorageFolder folder = storageItem as IStorageFolder;
        while (folder != null && folder.Path.AbsolutePath != ProjectRootFolder?.StorageFolderPath)
        {
            folder = await folder.GetParentAsync();
        }

        if (folder == null) return;

        string path = storageItem.Path.AbsolutePath.Replace(folder.Path.AbsolutePath, "");
        await ApiVault.Get().GetMainWindow().Clipboard.SetTextAsync(path);
    }

    private static void OpenInExplorer(IStorageItem storageItem)
    {
        if (storageItem is not IStorageFolder storageFolder) return;

        var path = storageFolder.Path.AbsolutePath;
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    #endregion

    #region Sorting

    private static void SortTabItem(TreeViewItem parent)
    {
        var folders = parent.Items
            .OfType<TabViewItem>()
            .Where(item => item.Tag is IStorageFolder)
            .OrderBy(item => item.Header);

        var files = parent.Items
            .OfType<TabViewItem>()
            .Where(item => item.Tag is IStorageFile)
            .OrderBy(item => item.Header);

        parent.Items.Clear();
        foreach (var folder in folders) parent.Items.Add(folder);
        foreach (var file in files) parent.Items.Add(file);
    }

    #endregion
}
