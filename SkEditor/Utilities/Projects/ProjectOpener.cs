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

        if (folder is null) return;

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

            //rootFolder.DoubleTapped += (sender, e) =>
            //{
            //    rootFolder.IsExpanded ^= true;
            //};

            // rootFolder.DoubleTapped will work also if you click on child items
            // So we need to add DoubleTapped event to this element: Avalonia.Controls.TreeViewItem /template/ Avalonia.Controls.Border#PART_LayoutRoot.TreeViewItemLayoutRoot

            //rootFolder.GetTemplateChildren("PART_LayoutRoot")!.DoubleTapped += (sender, e) =>
            //{
            //    rootFolder.IsExpanded ^= true;
            //};

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
                TreeViewItem item = new()
                {
                    Header = Path.GetFileName(path),
                    Tag = path,
                    FontWeight = FontWeight.Medium,
                };

                //item.DoubleTapped += (sender, e) =>
                //{
                //    item.IsExpanded ^= true;
                //};

                viewItem.Items.Add(item);

                AddChildren(item, storageFolder);
            }
            else
            {
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
}
