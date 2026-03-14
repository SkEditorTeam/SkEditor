using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Contracts.Models;
using SkEditor.Contracts.Services;
using SkEditor.Models.Projects;
using SkEditor.Models.Projects.Local;
using SkEditor.Models.Projects.Sftp;
using SkEditor.Models.Remote;

namespace SkEditor.ViewModels.Controls;

public partial class FileTreeViewModel(
    IRemoteConnectionService remoteConnectionService,
    INotificationService notificationService,
    ILogService logService)
    : ObservableObject
{
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private IFileSystem? _fileSystem;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<FileSystemNode> RootItems { get; } = [];

    public async Task<bool> ChangeConnection(RemoteConnection connection)
    {
        if (connection == null)
        {
            logService.Warning("Selected connection is null!");
            return false;
        }

        const bool isRemote = false;

        if (isRemote)
#pragma warning disable CS0162 // Unreachable code detected
        {
            FileSystem = new SftpFileSystem(connection, remoteConnectionService, notificationService, logService);
        }
        // ReSharper disable RedundantIfElseBlock
        else
        {
            FileSystem = new LocalFileSystem(@"C:\Users\Notro\Documents");
        }
#pragma warning restore CS0162 // Unreachable code detected

        bool success = await LoadFileSystemAsync();
        return success;
    }

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
            logService.Warning("File system is null, cannot load!");
            IsLoading = false;
            return false;
        }

        _cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        bool isSuccess = await FileSystem.Load(_cts.Token);

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

        logService.Info($"Loaded file system in {stopwatch.ElapsedMilliseconds} ms");

        IsLoading = false;
        return true;
    }
}