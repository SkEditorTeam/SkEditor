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

    private static EventHandler<TappedEventArgs>? _doubleTappedHandler;
    private static EventHandler<TappedEventArgs>? _tappedHandler;

    private static ExplorerSidebarPanel Panel => AddonLoader.GetCoreAddon().ProjectPanel.Panel;

    public static TreeView FileTreeView => Panel.FileTreeView;

    private static StackPanel NoFolderMessage => Panel.NoFolderMessage;

    public static async Task OpenProject(string? path = null)
    {
        string? folder = await ExtractFolderPath(path);
        if (string.IsNullOrEmpty(folder))
        {
            NoFolderMessage.IsVisible = true;
            return;
        }

        NoFolderMessage.IsVisible = false;
        ProjectRootFolder = new Folder(folder) { IsExpanded = true };
        FileTreeView.ItemsSource = new ObservableCollection<StorageElement> { ProjectRootFolder };
        SkEditorAPI.Events.ProjectOpened(ProjectRootFolder);

        RemoveEventHandlers();

        _doubleTappedHandler = (_, e) =>
        {
            if (SkEditorAPI.Core.GetAppConfig().IsProjectSingleClickEnabled)
            {
                return;
            }

            HandleTapped(e);
        };

        _tappedHandler = (_, e) =>
        {
            if (!SkEditorAPI.Core.GetAppConfig().IsProjectSingleClickEnabled)
            {
                return;
            }

            HandleTapped(e);
        };

        FileTreeView.DoubleTapped += _doubleTappedHandler;
        FileTreeView.Tapped += _tappedHandler;
    }

    private static async Task<string?> ExtractFolderPath(string? path)
    {
        string folder;

        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            TopLevel? topLevel = TopLevel.GetTopLevel(SkEditorAPI.Windows.GetMainWindow());
            if (topLevel == null)
            {
                NoFolderMessage.IsVisible = ProjectRootFolder == null;
                return string.Empty;
            }

            IReadOnlyList<IStorageFolder> folders =
                await topLevel.StorageProvider.OpenFolderPickerAsync(
                    new FolderPickerOpenOptions { AllowMultiple = false }
                );

            if (folders.Count == 0)
            {
                NoFolderMessage.IsVisible = ProjectRootFolder == null;
                return null;
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
                        string server = serverPart[..firstSlash];
                        string sharePath = serverPart[firstSlash..];

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
                        {
                            folder = folder[8..];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing folder path");

                string? rawString = folders[0].ToString();
                if (string.IsNullOrEmpty(rawString))
                {
                    NoFolderMessage.IsVisible = ProjectRootFolder == null;
                    return null;
                }

                int pathIndex = rawString.IndexOf("Path=", StringComparison.Ordinal);

                if (pathIndex >= 0)
                {
                    folder = rawString[(pathIndex + 5)..].Trim();

                    if (folder.StartsWith('\"') && folder.EndsWith('\"'))
                    {
                        folder = folder[1..^1];
                    }
                }
                else
                {
                    NoFolderMessage.IsVisible = ProjectRootFolder == null;

                    return null;
                }
            }
        }
        else
        {
            folder = path;
        }

        folder = folder.NormalizePathSeparators();
        return folder;
    }

    private static void HandleTapped(TappedEventArgs e)
    {
        if (e.Source is not Border border)
        {
            return;
        }

        TreeViewItem? treeViewItem = border.GetVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();

        if (treeViewItem is null)
        {
            return;
        }

        StorageElement? storageElement = treeViewItem.DataContext as StorageElement;

        storageElement?.HandleClick();
    }

    private static void RemoveEventHandlers()
    {
        if (_doubleTappedHandler != null)
        {
            FileTreeView.DoubleTapped -= _doubleTappedHandler;
            _doubleTappedHandler = null;
        }

        if (_tappedHandler == null)
        {
            return;
        }

        FileTreeView.Tapped -= _tappedHandler;
        _tappedHandler = null;
    }
}