using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using R3;
using SkEditor.Core.Extensions;
using SkEditor.Workspace.Filesystem.Abstractions;

namespace SkEditor.Workspace.Filesystem.Local;

public partial class LocalNodeFileSystem : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private DisposableBag _disposables;

    private FileSystemWatcher CreateWatcher(string directory)
    {
        FileSystemWatcher watcher = new(directory)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            InternalBufferSize = 65536,
            NotifyFilter = NotifyFilters.FileName |
                           NotifyFilters.DirectoryName |
                           NotifyFilters.LastWrite |
                           NotifyFilters.Size
        };

        SubscribeError(directory, watcher);
        SubscribeRenamed(watcher);
        SubscribeCreated(watcher);
        SubscribeDeleted(watcher);

        watcher.EnableRaisingEvents = true;
        return watcher;
    }

    private void SubscribeError(string directory, FileSystemWatcher watcher)
    {
        watcher.ErrorAsObservable()
            .SubscribeAwait(async (e, _) =>
            {
                _logs.Error(e.GetException(), "File system watcher error occurred");
                
                try
                {
                    watcher.EnableRaisingEvents = false;
                    if (Root is not null)
                    {
                        await Root.LoadChildrenAsync(true);
                    }
                }
                catch (Exception ex)
                {
                    _logs.Error(ex, "Failed to reload file system after watcher error");
                }
                finally
                {
                    if (Directory.Exists(directory))
                    {
                        watcher.EnableRaisingEvents = true;
                    }
                }
            }, AwaitOperation.Drop)
            .AddTo(ref _disposables);
    }

    private void SubscribeRenamed(FileSystemWatcher watcher)
    {
        watcher.RenamedAsObservable()
            .Delay(TimeSpan.FromMilliseconds(50))
            .ObserveOnCurrentSynchronizationContext()
            .Subscribe(e =>
            {
                if (e.Name is null)
                {
                    return;
                }

                bool newExists = File.Exists(e.FullPath) || Directory.Exists(e.FullPath);
                bool oldExists = File.Exists(e.OldFullPath) || Directory.Exists(e.OldFullPath);

                if (!newExists || oldExists)
                {
                    return;
                }

                string oldPath = e.OldFullPath;

                FileSystemNode? node = FindNode(oldPath);
                if (node is null)
                {
                    return;
                }

                string fileName = Path.GetFileName(e.FullPath);
                node.UpdateName(fileName);
            })
            .AddTo(ref _disposables);
    }

    private void SubscribeCreated(FileSystemWatcher watcher)
    {
        watcher.CreatedAsObservable()
            .Delay(TimeSpan.FromMilliseconds(50))
            .ObserveOnCurrentSynchronizationContext()
            .Subscribe(e =>
            {
                if (e.Name is null)
                {
                    return;
                }

                string path = e.FullPath;
                bool exists = File.Exists(path) || Directory.Exists(path);

                if (!exists)
                {
                    return;
                }

                string parentPath = Path.GetDirectoryName(e.FullPath) ?? string.Empty;
                FileSystemNode? parentNode = FindNode(parentPath);
                if (parentNode is not LocalFolderNode folderNode)
                {
                    return;
                }

                if (!folderNode.HasLoadedChildren)
                {
                    return;
                }

                if (folderNode.Children.Any(c => c.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                FileAttributes attributes = File.GetAttributes(path);
                bool isDir = attributes.HasFlag(FileAttributes.Directory);

                string name = Path.GetFileName(e.FullPath);
                FileSystemNode newNode = isDir
                    ? new LocalFolderNode(name, path, false)
                    : new LocalFileNode(name, path);

                folderNode.AddChild(newNode);
            })
            .AddTo(ref _disposables);
    }

    private void SubscribeDeleted(FileSystemWatcher watcher)
    {
        watcher.DeletedAsObservable()
            .Delay(TimeSpan.FromMilliseconds(50))
            .ObserveOnCurrentSynchronizationContext()
            .Subscribe(e =>
            {
                if (e.Name is null)
                {
                    return;
                }

                string path = e.FullPath;
                bool exists = File.Exists(path) || Directory.Exists(path);

                if (exists)
                {
                    return;
                }

                string parentPath = Path.GetDirectoryName(e.FullPath) ?? string.Empty;
                FileSystemNode? parentNode = FindNode(parentPath);
                if (parentNode is not LocalFolderNode folderNode)
                {
                    return;
                }

                if (!folderNode.HasLoadedChildren)
                {
                    return;
                }

                FileSystemNode? nodeToRemove = FindNode(path);
                if (nodeToRemove is null)
                {
                    return;
                }

                folderNode.RemoveChild(nodeToRemove);
            })
            .AddTo(ref _disposables);
    }
    
    private FileSystemNode? FindNode(string path)
    {
        if (Root is null)
        {
            return null;
        }

        ReadOnlySpan<char> normalizedRootPath = GetNormalizedPath(Root.Path);
        ReadOnlySpan<char> normalizedPath = GetNormalizedPath(path);

        bool isRoot = normalizedPath.Equals(normalizedRootPath, StringComparison.OrdinalIgnoreCase);
        if (isRoot)
        {
            return Root;
        }

        FileSystemNode? node = FindNode(normalizedPath, Root.Children);
        return node;
    }

    private static FileSystemNode? FindNode(ReadOnlySpan<char> path, IEnumerable<FileSystemNode> children)
    {
        foreach (FileSystemNode child in children)
        {
            bool same = path.Equals(child.Path.AsSpan(), StringComparison.OrdinalIgnoreCase);

            if (same)
            {
                return child;
            }

            if (child is not { IsDirectory: true, HasLoadedChildren: true })
            {
                continue;
            }

            FileSystemNode? found = FindNode(path, child.Children);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    private static ReadOnlySpan<char> GetNormalizedPath(string path)
    {
        return Path.GetFullPath(path)
            .AsSpan()
            .TrimEnd(Path.DirectorySeparatorChar)
            .TrimEnd(Path.AltDirectorySeparatorChar);
    }

    public void Dispose()
    {
        _watcher.Dispose();
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }
}