using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Files;

namespace SkEditor.Utilities.Projects;
public static class ProjectOpener
{
    private static TreeView FileTreeView => ApiVault.Get().GetMainWindow().SideBar.FileTreeView;
    private static Grid ProjectInformation => ApiVault.Get().GetMainWindow().SideBar.ProjectInformation;
    private static IStorageFolder ProjectRootFolder;

    public static async void OpenProject()
    {
        
        TopLevel topLevel = TopLevel.GetTopLevel(ApiVault.Get().GetMainWindow());

        IReadOnlyList<IStorageFolder> folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            SuggestedStartLocation = await topLevel.StorageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Desktop),
            AllowMultiple = false
        });

        if (folder.Count == 0) 
            return;
        
        if (ProjectInformation.IsVisible) ProjectInformation.IsVisible = false;
        if (!FileTreeView.IsVisible) FileTreeView.IsVisible = true;

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

    private static async void AddChildren(TreeViewItem viewItem, IStorageFolder folder)
    {
        await foreach (IStorageItem storageItem in folder.GetItemsAsync())
        {
            if (storageItem is IStorageFolder storageFolder)
            {
                TreeViewItem folderItem = CreateTreeViewItem(storageFolder);
                viewItem.Items.Add(folderItem);
                
                AddChildren(folderItem, storageFolder);
            }
            else
            {
                TreeViewItem item = CreateTreeViewItem(storageItem);

                viewItem.Items.Add(item);
            }
        }
    }

    #region TabViewItem Creation

    public static TreeViewItem CreateTreeViewItem(IStorageItem storageItem, bool root = false)
    {
        bool isFolder = storageItem is IStorageFolder;
        TreeViewItem item = new TreeViewItem
        {
            IsExpanded = false,
            Header = CreateTreeItemHeader(storageItem, root),
            Tag = storageItem,
            FontWeight = isFolder ? FontWeight.Medium : FontWeight.Normal
        };

        item.ContextFlyout = CreateContextMenu(item, storageItem);

        if (!isFolder)
        {
            item.DoubleTapped += (sender, e) =>
            {
                FileHandler.OpenFile(storageItem.Path.AbsolutePath);
            };
        }
        
        return item;
    }
    
    private static object CreateTreeItemHeader(IStorageItem storageItem, bool root = false)
    {
        bool isFolder = storageItem is IStorageFolder;
        StackPanel stackPanel = new StackPanel() { Orientation = Orientation.Horizontal };
        Label label = new Label() { Content = storageItem.Name };

        Control icon = root ? new SymbolIcon()
        { 
            Symbol = Symbol.Home,
            FontSize = IconSize
        } : GetFileIcon(isFolder, Path.GetExtension(storageItem.Name)) as Control;
        
        stackPanel.Children.Add(icon);
        stackPanel.Children.Add(label);
        
        return stackPanel;
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
            {
                Task.Delay(5).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(() => parent.IsExpanded = true));
                onValidation((item.Header as TextBox).Text);
            } else if (args.Key == Key.Escape)
            {
                parent.Items.Remove(item);
            }
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
            new { Header = "MenuHeaderCopyPath", Command = new RelayCommand(() => CopyPath(storageItem)), Icon = Symbol.Copy },
            new { Header = "MenuHeaderCopyAbsolutePath", Command = new RelayCommand(() => CopyPath(storageItem, true)), Icon = Symbol.Copy },
            null,
            new { Header = "MenuHeaderRename", Command = new RelayCommand(() => Rename(treeViewItem, storageItem)), Icon = Symbol.Rename },
            new { Header = "MenuHeaderDelete", Command = new RelayCommand(() => DeleteItem(treeViewItem, storageItem)), Icon = Symbol.Delete }
        }.ToList();
        
        if (storageItem is IStorageFolder)
        {
            commands.Insert(0, new { Header = "MenuHeaderNewFile", Command = new RelayCommand(() => CreateElement(treeViewItem, storageItem)), Icon = Symbol.New });
            commands.Insert(1, new { Header = "MenuHeaderNewFolder", Command = new RelayCommand(() => CreateElement(treeViewItem, storageItem, true)), Icon = Symbol.NewFolder });
            commands.Insert(2, new { Header = "MenuHeaderOpenInExplorer", Command = new RelayCommand(() => OpenFolder(storageItem)), Icon = Symbol.OpenFolder });
            commands.Insert(3, null);
        }

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
    
    private static async void DeleteItem(TreeViewItem treeViewItem, IStorageItem storageItem)
    {
        await storageItem.DeleteAsync();
        (treeViewItem.Parent as TreeViewItem).Items.Remove(treeViewItem);
    }

    private static async void Rename(TreeViewItem treeViewItem, IStorageItem item)
    {
        var previousHeader = treeViewItem.Header;
        treeViewItem.Header = new TextBox()
        {
            Text = item.Name,
            IsReadOnly = false,
            Width = 150
        };
        
        (treeViewItem.Header as TextBox).LostFocus += (_, _) =>
        {
            treeViewItem.Header = previousHeader;
        };
        
        (treeViewItem.Header as TextBox).KeyDown += async (_, args) =>
        {
            if (args.Key == Key.Enter)
            {
                string text = (treeViewItem.Header as TextBox).Text;
                var newPath = item.Path.AbsolutePath.Replace(item.Name, text);
                if (File.Exists(newPath) || Directory.Exists(newPath))
                {
                    await ApiVault.Get().ShowMessageWithIcon("Error", "A file/folder with this name already exists.", new SymbolIconSource() { Symbol = Symbol.Alert});
                    return;
                }

                var parent = treeViewItem.Parent as TreeViewItem;
                var storageParent = await item.GetParentAsync();
                parent.Items.Remove(treeViewItem);
                if (item is IStorageFile)
                {
                    Task.Delay(5).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(() => parent.IsExpanded = true));
                    RenameFile(item.Path.AbsolutePath, text);

                    IStorageFile newFile = null;
                    await foreach (var iFile in storageParent.GetItemsAsync())
                    {
                        if (!(iFile is IStorageFile file)) continue;
                        if (file.Name == text)
                        {
                            newFile = file;
                            break;
                        }
                    }
                    
                    var newItem = CreateTreeViewItem(newFile);
                    newItem.ContextFlyout = CreateContextMenu(newItem, newFile);
                    parent.Items.Add(newItem);
                    SortTabItem(parent);
                }
                else
                {
                    Task.Delay(5).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(() => parent.IsExpanded = true));
                    RenameFolder(item.Path.AbsolutePath, text);
                    
                    IStorageFolder newFolder = null;
                    await foreach (var iFolder in storageParent.GetItemsAsync())
                    {
                        if (!(iFolder is IStorageFolder folder)) continue;
                        if (folder.Name == text)
                        {
                            newFolder = folder;
                            break;
                        }
                    }
                    
                    var newItem = CreateTreeViewItem(newFolder);
                    newItem.ContextFlyout = CreateContextMenu(newItem, newFolder);
                    parent.Items.Add(newItem);
                    SortTabItem(parent);
                }
                
            } else if (args.Key == Key.Escape)
            {
                treeViewItem.Header = previousHeader;
            }
        };
    }
    
    public static void RenameFile(string filePath, string newName)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        fileInfo.MoveTo(fileInfo.Directory.FullName + "\\" + newName);
    }
    
    public static void RenameFolder(string folderPath, string newName)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
        directoryInfo.MoveTo(directoryInfo.Parent.FullName + "\\" + newName);
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
                {
                    var folder = await storageFolder.CreateFolderAsync(name);
                    
                    TreeViewItem folderItem = CreateTreeViewItem(folder);
                    folderItem.ContextFlyout = CreateContextMenu(folderItem, folder);
                    
                    int index = 0;
                    foreach (var child in treeViewItem.Items)
                    {
                        if (child is TreeViewItem { Tag: IStorageFolder } childItem)
                        {
                            if (String.Compare((childItem.Tag as IStorageFolder).Name, name, StringComparison.Ordinal) > 0)
                                break;
                            index++;
                        }
                    }
                    
                    treeViewItem.Items.Insert(index + 1, folderItem);
                }
                else
                {
                    var file = await storageFolder.CreateFileAsync(name);
                    
                    TreeViewItem fileItem = CreateTreeViewItem(file);
                    fileItem.ContextFlyout = CreateContextMenu(fileItem, file);
                    
                    // Fint (alphabetically) the right index to put the new file
                    int index = 0;
                    foreach (var child in treeViewItem.Items)
                    {
                        if (child is TreeViewItem { Tag: IStorageFile } childItem)
                        {
                            if (String.Compare((childItem.Tag as IStorageFile).Name, name, StringComparison.Ordinal) > 0)
                                break;
                            index++;
                        }
                    }
                    
                    treeViewItem.Items.Insert(index + 1, fileItem);
                    FileHandler.OpenFile(file.Path.AbsolutePath);
                }
            });
            
            treeViewItem.Items.Insert(0, item);
            item.Focus(NavigationMethod.Pointer);
        }
    }

    private static void OpenFolder(IStorageItem storageItem)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = storageItem.Path.AbsolutePath,
            UseShellExecute = true,
            Verb = "open"
        });
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
