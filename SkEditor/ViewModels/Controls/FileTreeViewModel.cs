using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Contracts.Models;
using SkEditor.Contracts.Services;
using SkEditor.Models.Projects;
using SkEditor.Models.Remote;

namespace SkEditor.ViewModels.Controls;

public partial class FileTreeViewModel : ObservableObject
{
    [ObservableProperty]
    private IFileSystem _fileSystem;

    public FileTreeViewModel(IRemoteConnectionService remoteConnectionService, INotificationService notificationService)
    {
        RemoteConnection? first = SkEditorAPI.Configs.GetConnectionsConfig().Connections.FirstOrDefault();

        if (first == null)
        {
            SkEditorAPI.Logs.Warning("No remote connections found in config!");
            throw new InvalidOperationException("No remote connections found in config!");
        }

        _fileSystem = new SftpFileSystem(first, remoteConnectionService, notificationService);
    }

    public ObservableCollection<FileSystemNode> RootItems { get; } = [];


    [RelayCommand]
    public async Task LoadFileSystemAsync()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        await FileSystem.Load();

        stopwatch.Stop();

        if (FileSystem.Root != null)
        {
            RootItems.Add(FileSystem.Root);
        }

        SkEditorAPI.Logs.Info($"Loaded file system in {stopwatch.ElapsedMilliseconds} ms");
    }
}