using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkEditor.Utilities.Projects;
public static class ProjectOpener
{
    private static TreeView FileTreeView => ApiVault.Get().GetMainWindow().SideBar.FileTreeView;

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
            TreeViewItem rootFolder = new()
            {
                Header = storageFolder.Name,
                Tag = storageFolder.Path,
                IsExpanded = true,
                FontWeight = FontWeight.SemiBold
            };

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
                TreeViewItem folderItem = new()
                {
                    Header = Path.GetFileName(path),
                    Tag = path,
                    FontWeight = FontWeight.Medium,
                };

                viewItem.Items.Add(folderItem);

                AddChildren(folderItem, storageFolder);
                return;
            }

            TreeViewItem item = new()
            {
                Header = Path.GetFileName(path),
                Tag = path,
                FontWeight = FontWeight.Normal
            };

            item.DoubleTapped += (sender, e) =>
            {
                FileHandler.OpenFile(path);
            };

            viewItem.Items.Add(item);
        }
    }
}
