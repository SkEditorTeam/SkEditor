using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Models.Projects.Types;
using SkEditor.Utilities.Projects;

namespace SkEditor.Models.Projects.Local;

public partial class LocalFileSystem : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private DisposableBag _disposables;

    private static INotificationService Notifs => App.Services.GetRequiredService<INotificationService>();

    public void Dispose()
    {
        _watcher.Dispose();
        _disposables.Dispose();
        GC.SuppressFinalize(this);
    }

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

        watcher.ErrorAsObservable()
            .SubscribeAwait(async (_, _) =>
            {
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
                    Notifs.Debug($"Error {ex.Message}");
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

        watcher.RenamedAsObservable()
            .Subscribe(e =>
            {
                if (e.Name is null)
                {
                    return;
                }

                string oldPath = e.OldFullPath;

                FileSystemNode? node = FindNode(oldPath);
                if (node is null)
                {
                    Notifs.Debug($"Renamed node not found for path: {oldPath}");
                }

                node?.UpdateName(e.Name);
            })
            .AddTo(ref _disposables);

        watcher.EnableRaisingEvents = true;
        return watcher;
    }

    private FileSystemNode? FindNode(string path)
    {
        if (Root is null)
        {
            return null;
        }

        FileSystemNode? node = FindNode(path, Root.Children);
        return node;
    }

    private static FileSystemNode? FindNode(string path, IEnumerable<FileSystemNode> children)
    {
        foreach (FileSystemNode child in children)
        {
            bool same = GetNormalizedPath(path)
                .Equals(GetNormalizedPath(child.Path), StringComparison.OrdinalIgnoreCase);

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
}