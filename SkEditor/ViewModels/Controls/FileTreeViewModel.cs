using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Contracts.Models;
using SkEditor.Contracts.Services;
using SkEditor.Models.Projects;
using SkEditor.Models.Remote;

namespace SkEditor.ViewModels.Controls;

public partial class FileTreeViewModel(
    IRemoteConnectionService remoteConnectionService,
    INotificationService notificationService)
    : ObservableObject
{
    [ObservableProperty]
    private IFileSystem? _fileSystem;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<FileSystemNode> RootItems { get; } = [];

    public async Task ChangeConnection(RemoteConnection connection)
    {
        if (connection == null)
        {
            SkEditorAPI.Logs.Warning("Selected connection is null!");
            return;
        }

        FileSystem = new SftpFileSystem(connection, remoteConnectionService, notificationService);
        await LoadFileSystemAsync();
    }

    [RelayCommand]
    public async Task LoadFileSystemAsync()
    {
        RootItems.Clear();
        IsLoading = true;

        Stopwatch stopwatch = Stopwatch.StartNew();

        if (FileSystem == null)
        {
            SkEditorAPI.Logs.Warning("File system is null, cannot load!");
            return;
        }

        await FileSystem.Load();

        stopwatch.Stop();

        if (FileSystem.Root != null)
        {
            RootItems.Add(FileSystem.Root);
        }

        SkEditorAPI.Logs.Info($"Loaded file system in {stopwatch.ElapsedMilliseconds} ms");

        IsLoading = false;
    }
}