using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Models;
using SkEditor.Contracts.Services;
using SkEditor.Models.Projects;
using SkEditor.Utilities;

namespace SkEditor.ViewModels.Controls;

public partial class FileTreeViewModel(ILogService logs, INotificationService notifs) : ObservableObject
{
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    public partial IFileSystem? FileSystem { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public ObservableCollection<FileSystemNode> RootItems { get; } = [];

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
            logs.Warning("File system is null, cannot load!");
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

    [RelayCommand]
    public void OpenInExplorer(FileSystemNode node)
    {
        if (node.IsDirectory)
        {
            LinkOpener.OpenFolder(node.Path);
        }
        else
        {
            LinkOpener.OpenFolderWithFileSelection(node.Path);
        }
    }

    [RelayCommand]
    public void NewFile(FileSystemNode node)
    {
        if (!node.IsDirectory)
        {
            return;
        }

        notifs.Debug("New file creation is not implemented yet.");
    }

    [RelayCommand]
    public void NewFolder(FileSystemNode node)
    {
        if (!node.IsDirectory)
        {
            return;
        }

        notifs.Debug("New folder creation is not implemented yet.");
    }

    [RelayCommand]
    public void Rename(FileSystemNode node)
    {
        notifs.Debug("Renaming is not implemented yet.");
    }

    [RelayCommand]
    public void Delete(FileSystemNode node)
    {
        notifs.Debug("Delete is not implemented yet.");
    }

    [RelayCommand]
    public void CopyPath(FileSystemNode node)
    {
        notifs.Debug("CopyPath is not implemented yet.");
    }

    [RelayCommand]
    public void CopyRelativePath(FileSystemNode node)
    {
        notifs.Debug("CopyRelativePath is not implemented yet.");
    }

    [RelayCommand]
    public void Download(FileSystemNode node)
    {
        notifs.Debug("Download is not implemented yet.");
    }

    [RelayCommand]
    public void Refresh(FileSystemNode node)
    {
        notifs.Debug("Refresh is not implemented yet.");
    }

    [RelayCommand]
    public void EditConnection(FileSystemNode node)
    {
        notifs.Debug("EditConnection is not implemented yet.");
    }

    [RelayCommand]
    public void CloseWorkspace(FileSystemNode node)
    {
        notifs.Debug("CloseWorkspace is not implemented yet.");
    }

    [RelayCommand]
    public void Disconnect(FileSystemNode node)
    {
        notifs.Debug("Disconnect is not implemented yet.");
    }
}