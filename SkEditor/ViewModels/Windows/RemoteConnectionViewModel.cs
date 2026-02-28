using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Renci.SshNet;
using SkEditor.API;

namespace SkEditor.ViewModels.Windows;

public partial class RemoteConnectionViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled))]
    private string _host = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled))]
    private string _port = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled))]
    private string _username = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled))]
    private string _password = "";

    [ObservableProperty]
    private string? _infoBarMessage;

    [ObservableProperty]
    private InfoBarSeverity _infoBarSeverity = InfoBarSeverity.Informational;

    public bool RequiredFieldsFilled =>
        !string.IsNullOrWhiteSpace(Host) &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password);

    public AsyncRelayCommand TestConnectionCommand { get; }
    public AsyncRelayCommand AddConnectionCommand { get; }

    public event EventHandler? CloseRequested;

    public RemoteConnectionViewModel()
    {
        TestConnectionCommand = new AsyncRelayCommand(TestConnection);
        AddConnectionCommand = new AsyncRelayCommand(AddConnection);
    }

    private async Task AddConnection()
    {
        bool connectionSuccessful = await TestConnection();
        if (!connectionSuccessful)
        {
            InfoBarMessage = "Cannot add connection. Please fix the connection issues first.";
            InfoBarSeverity = InfoBarSeverity.Error;

            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            SkEditorAPI.Windows.GetMainWindow()?.NotificationManager.Show(
                "Connection added!",
                $"Successfully added connection to {Host}:{(!string.IsNullOrWhiteSpace(Port) ? Port : 22)}.",
                NotificationType.Success,
                TimeSpan.FromSeconds(5)
            );

            CloseRequested?.Invoke(this, EventArgs.Empty);
            Clear();
        });
    }

    private async Task<bool> TestConnection()
    {
        string host = Host;
        int port = int.TryParse(Port, out int p) ? p : 22;
        string username = Username;
        string password = Password;

        using var client = new SftpClient(host, port, username, password);

        try
        {
            using var cts = new CancellationTokenSource();

            Stopwatch sw = Stopwatch.StartNew();
            await client.ConnectAsync(cts.Token);
            sw.Stop();

            if (client.IsConnected)
            {
                InfoBarMessage = $"Successfully connected to {host}:{port} in {sw.ElapsedMilliseconds} ms.";
                InfoBarSeverity = InfoBarSeverity.Success;
                return true;
            }

            InfoBarMessage = $"Failed to connect to {host}:{port}.";
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

    private void Clear()
    {
        Host = "";
        Port = "";
        Username = "";
        Password = "";
        InfoBarMessage = null;
        InfoBarSeverity = InfoBarSeverity.Informational;
    }
}