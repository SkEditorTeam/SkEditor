using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Renci.SshNet;
using Renci.SshNet.Common;
using SkEditor.API;
using SkEditor.Contracts.Services;
using SkEditor.Models.Remote;
using SkEditor.Services;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Windows;

public partial class RemoteConnectionViewModel(IRemoteConnectionService connectionService) : ObservableObject
{
    [ObservableProperty]
    private RemoteConnectionInfo _connectionInfo = new();
    
    [ObservableProperty]
    private string? _infoBarMessage;

    [ObservableProperty]
    private InfoBarSeverity _infoBarSeverity = InfoBarSeverity.Informational;

    public IReadOnlyList<RemoteAuthMethod> AuthMethods { get; } =
        Enum.GetValues<RemoteAuthMethod>().ToList();

    public event EventHandler? CloseRequested;
    public Func<Task<string?>>? RequestKeyFilePicker { get; set; }

    [RelayCommand]
    private async Task AddConnection()
    {
        bool connectionSuccessful = await TestConnection();
        if (!connectionSuccessful)
        {
            InfoBarMessage = Translation.Get(TranslationKey.RemoteConnectionCannotAddConnection);
            InfoBarSeverity = InfoBarSeverity.Error;

            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            string hostPort = $"{ConnectionInfo.Host}:{(!string.IsNullOrWhiteSpace(ConnectionInfo.Port) ? ConnectionInfo.Port : "22")}";

            SkEditorAPI.Windows.GetMainWindow()?.NotificationManager.Show(
                Translation.Get(TranslationKey.RemoteConnectionAddSuccessTitle),
                Translation.Get(TranslationKey.RemoteConnectionAddSuccessMessage, hostPort),
                NotificationType.Success,
                TimeSpan.FromSeconds(5)
            );

            CloseRequested?.Invoke(this, EventArgs.Empty);
            Clear();
        });
    }

    [RelayCommand]
    private async Task<bool> TestConnection()
    {
        connectionService.Test();
        return true;
        
        string host = ConnectionInfo.Host;
        int port = int.TryParse(ConnectionInfo.Port, out int p) ? p : 22;
        string username = ConnectionInfo.Username;
        string password = ConnectionInfo.Password;
        string keyFilePath = ConnectionInfo.KeyFilePath;

        bool invalidKeyFile = ConnectionInfo.IsPrivateKeyAuth && (string.IsNullOrWhiteSpace(keyFilePath) || !File.Exists(keyFilePath));
        if (invalidKeyFile)
        {
            InfoBarMessage = Translation.Get(TranslationKey.RemoteConnectionInvalidKeyPath);
            InfoBarSeverity = InfoBarSeverity.Error;
            return false;
        }

        try
        {
            AuthenticationMethod authMethod = ConnectionInfo.IsPasswordAuth
                ? new PasswordAuthenticationMethod(username, password)
                : new PrivateKeyAuthenticationMethod(username, new PrivateKeyFile(keyFilePath));

            var connectionInfo = new ConnectionInfo(host, port, username, authMethod);

            using var client = new SftpClient(connectionInfo);

            using var cts = new CancellationTokenSource();

            Stopwatch sw = Stopwatch.StartNew();
            await client.ConnectAsync(cts.Token);
            sw.Stop();

            string hostPort = $"{host}:{port}";
            if (client.IsConnected)
            {
                InfoBarMessage = Translation.Get(TranslationKey.RemoteConnectionTestSuccess, hostPort,
                    $"{sw.ElapsedMilliseconds:N0}");
                InfoBarSeverity = InfoBarSeverity.Success;
                return true;
            }

            InfoBarMessage = Translation.Get(TranslationKey.RemoteConnectionTestFailed, hostPort);
            InfoBarSeverity = InfoBarSeverity.Error;
            return false;
        }
        catch (SshException ex) when (ex.Message == "Invalid private key file.")
        {
            InfoBarMessage = Translation.Get(TranslationKey.RemoteConnectionInvalidKeyFormat);
            InfoBarSeverity = InfoBarSeverity.Error;
            return false;
        }
        catch (Exception ex)
        {
            InfoBarMessage = ex.Message;
            InfoBarSeverity = InfoBarSeverity.Error;
            return false;
        }
    }

    [RelayCommand]
    private async Task BrowseKey()
    {
        if (RequestKeyFilePicker == null) return;

        var path = await RequestKeyFilePicker.Invoke();

        if (!string.IsNullOrEmpty(path))
        {
            ConnectionInfo.KeyFilePath = path;
        }
    }

    private void Clear()
    {
        ConnectionInfo = new RemoteConnectionInfo();
        InfoBarMessage = null;
        InfoBarSeverity = InfoBarSeverity.Informational;
    }
}