using System;
using System.Collections.Generic;
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
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private IFileSystem _fileSystem;

    [ObservableProperty]
    private bool _isLoading;

    public FileTreeViewModel(IRemoteConnectionService remoteConnectionService, INotificationService notificationService)
    {
        _notificationService = notificationService;

        if (false)
        {
            RemoteConnection? first = SkEditorAPI.Configs.GetConnectionsConfig().Connections.FirstOrDefault();

            if (first == null)
            {
                SkEditorAPI.Logs.Warning("No remote connections found in config!");
                throw new InvalidOperationException("No remote connections found in config!");
            }

            _fileSystem = new SftpFileSystem(first, remoteConnectionService, notificationService);
        }
    }

    public ObservableCollection<FileSystemNode> RootItems { get; } = [];

    public ObservableCollection<RemoteConnection> RemoteConnections { get; } =
        new(SkEditorAPI.Configs.GetConnectionsConfig().Connections);

    public RemoteConnection SelectedConnection
    {
        get;
        set
        {
            if (Equals(value, field))
            {
                return;
            }

            field = value;
            _ = ChangeConnection(value);
        }
    }

    public bool IsEmpty => RootItems.Count == 0;

    public List<ConnectionMenuItem> ConnectionMenuItems =>
        RemoteConnections
            .Select(c => new ConnectionMenuItem(c, ConnectToSelectedCommand))
            .ToList();

    public void AddConnection(RemoteConnection connection)
    {
        RemoteConnections.Add(connection);
        OnPropertyChanged(nameof(ConnectionMenuItems));
        OnPropertyChanged(nameof(IsEmpty));
    }

    public async Task ChangeConnection(RemoteConnection connection)
    {
        if (connection == null)
        {
            SkEditorAPI.Logs.Warning("Selected connection is null!");
            return;
        }

        _notificationService.Show("Changing Connection", $"Changing connection to {connection.Name}...");
    }

    [RelayCommand]
    public async Task ConnectToLast()
    {
        _notificationService.Show("Last", "connecting to last!");
    }

    [RelayCommand]
    public async Task ConnectToSelected(RemoteConnection connection)
    {
        _notificationService.Show("Selected", $"connecting to {connection.Name}!");
    }

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

    public record ConnectionMenuItem(RemoteConnection Connection, IRelayCommand<RemoteConnection> ConnectCommand)
    {
        public string Name => Connection.Name;
    }
}