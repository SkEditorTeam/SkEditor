using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using Serilog;
using SkEditor.API;
using SkEditor.Controls.Sidebar;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Utilities.Projects.Elements;

namespace SkEditor.Utilities.Projects;

public static class ProjectOpener
{
    public static Folder? ProjectRootFolder;

    private static ExplorerSidebarPanel Panel => AddonLoader.GetCoreAddon().ProjectPanel.Panel;

    public static TreeView FileTreeView => Panel.FileTreeView;

    private static StackPanel NoFolderMessage => Panel.NoFolderMessage;

    public static async Task OpenProject(string? path = null)
    {
        string folder;

        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            TopLevel topLevel = TopLevel.GetTopLevel(SkEditorAPI.Windows.GetMainWindow());

            IReadOnlyList<IStorageFolder> folders =
                await topLevel.StorageProvider.OpenFolderPickerAsync(
                    new FolderPickerOpenOptions { AllowMultiple = false }
                );

            if (folders.Count == 0)
            {
                NoFolderMessage.IsVisible = ProjectRootFolder == null;
                return;
            }

            try
            {
                string rawPath = folders[0].Path.ToString();

                if (rawPath.StartsWith("file://") && !rawPath.StartsWith("file:///"))
                {
                    string serverPart = rawPath[7..];

                    int firstSlash = serverPart.IndexOf('/');

                    if (firstSlash >= 0)
                    {
                        string server = serverPart.Substring(0, firstSlash);
                        string sharePath = serverPart.Substring(firstSlash);

                        folder = $@"\\{server}{sharePath}";
                    }
                    else
                    {
                        folder = $@"\\{serverPart}";
                    }
                }
                else
                {
                    try
                    {
                        folder = folders[0].Path.AbsolutePath;
                    }
                    catch
                    {
                        folder = rawPath;

                        if (folder.StartsWith("file:///"))
                            folder = folder[8..];
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing folder path");

                string rawString = folders[0].ToString();

                int pathIndex = rawString.IndexOf("Path=", StringComparison.Ordinal);

                if (pathIndex >= 0)
                {
                    folder = rawString[(pathIndex + 5)..].Trim();

                    if (folder.StartsWith('\"') && folder.EndsWith('\"'))
                        folder = folder[1..^1];
                }
                else
                {
                    NoFolderMessage.IsVisible = ProjectRootFolder == null;

                    return;
                }
            }
        }
        else
        {
            folder = path;
        }

        folder = folder.NormalizePathSeparators();

        NoFolderMessage.IsVisible = false;
        ProjectRootFolder = new Folder(folder) { IsExpanded = true };
        FileTreeView.ItemsSource = new ObservableCollection<StorageElement> { ProjectRootFolder };

        static void HandleTapped(TappedEventArgs e)
        {
            if (e.Source is not Border border)
                return;

            var treeViewItem = border.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();

            if (treeViewItem is null)
                return;

            var storageElement = treeViewItem.DataContext as StorageElement;

            storageElement?.HandleClick();
        }

        FileTreeView.DoubleTapped += (_, e) =>
        {
            if (SkEditorAPI.Core.GetAppConfig().IsProjectSingleClickEnabled)
                return;

            HandleTapped(e);
        };

        FileTreeView.Tapped += (_, e) =>
        {
            if (!SkEditorAPI.Core.GetAppConfig().IsProjectSingleClickEnabled)
                return;

            HandleTapped(e);
        };
    }

    #region Sorting



    private static void SortTabItem(TreeViewItem parent)
    {
        var folders = parent
            .Items.OfType<TabViewItem>()
            .Where(item => item.Tag is IStorageFolder)
            .OrderBy(item => item.Header);

        var files = parent
            .Items.OfType<TabViewItem>()
            .Where(item => item.Tag is IStorageFile)
            .OrderBy(item => item.Header);

        parent.Items.Clear();

        foreach (var folder in folders)
            parent.Items.Add(folder);

        foreach (var file in files)
            parent.Items.Add(file);
    }

    #endregion
}