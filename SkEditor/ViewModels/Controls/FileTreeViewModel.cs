using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Contracts.Models;
using SkEditor.Contracts.Services;
using SkEditor.Models.Projects;

namespace SkEditor.ViewModels.Controls;

public partial class FileTreeViewModel : ObservableObject
{
    private readonly ILogService _logs;
    private readonly INotificationService _notifs;
    private CancellationTokenSource? _cts;

    public FileTreeViewModel(ILogService logs, INotificationService notifs)
    {
        _logs = logs;
        _notifs = notifs;

        SelectedNodes.CollectionChanged += OnSelectedNodesChanged;
    }

    [ObservableProperty]
    public partial IFileSystem? FileSystem { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public ObservableCollection<FileSystemNode> RootItems { get; } = [];

    public ObservableCollection<FileSystemNode> SelectedNodes { get; } = [];

    public ObservableCollection<ContextMenuItem> ContextMenuItems { get; } = [];

    /// <summary>
    ///     Loads the file system and populates the root items.
    /// </summary>
    /// <returns>True if success</returns>
    public async Task<bool> LoadFileSystemAsync()
    {
        if (_cts != null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
            _cts = null;
        }

        RootItems.Clear();
        IsLoading = true;

        Stopwatch stopwatch = Stopwatch.StartNew();

        if (FileSystem == null)
        {
            _logs.Warning("File system is null, cannot load!");
            IsLoading = false;
            return false;
        }

        _cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        bool isSuccess = await FileSystem.LoadAsync(_cts.Token);

        if (!isSuccess)
        {
            IsLoading = false;
            return false;
        }

        stopwatch.Stop();

        if (FileSystem.Root != null)
        {
            RootItems.Clear();
            RootItems.Add(FileSystem.Root);
        }

        IsLoading = false;
        return true;
    }

    public void OnSelectedNodesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateContextMenuItems();
    }

    public void UpdateContextMenuItems()
    {
        ContextMenuItems.Clear();

        if (SelectedNodes.Count == 0)
        {
            return;
        }

        List<ContextMenuItem> templateMenu = SelectedNodes.First().GetMenuItems();

        HashSet<string?> commonHeaders = SelectedNodes
            .Select(node => node.GetMenuItems().Where(m => !m.IsSeparator).Select(m => m.Header))
            .Aggregate((prev, next) => prev.Intersect(next).ToList())
            .ToHashSet();

        List<ContextMenuItem> filteredMenu = templateMenu
            .Where(item => item.IsSeparator || commonHeaders.Contains(item.Header))
            .ToList();

        ContextMenuItems.AddRange(CleanUpSeparators(filteredMenu));
    }

    public List<ContextMenuItem> CleanUpSeparators(IEnumerable<ContextMenuItem> menuItems)
    {
        List<ContextMenuItem> cleaned = new();
        bool lastWasSeparator = true;

        foreach (ContextMenuItem item in menuItems)
        {
            if (item.IsSeparator)
            {
                if (lastWasSeparator)
                {
                    continue;
                }

                cleaned.Add(item);
                lastWasSeparator = true;
            }
            else
            {
                cleaned.Add(item);
                lastWasSeparator = false;
            }
        }

        if (cleaned.Count > 0 && cleaned[^1].IsSeparator)
        {
            cleaned.RemoveAt(cleaned.Count - 1);
        }

        return cleaned;
    }
}