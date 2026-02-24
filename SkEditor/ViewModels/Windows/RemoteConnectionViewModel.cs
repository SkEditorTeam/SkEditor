using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
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
        !string.IsNullOrWhiteSpace(Port) &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Password);

    public AsyncRelayCommand TestConnectionCommand { get; }
    public AsyncRelayCommand AddConnectionCommand { get; }

    public RemoteConnectionViewModel()
    {
        TestConnectionCommand = new AsyncRelayCommand(TestConnection);
        AddConnectionCommand = new AsyncRelayCommand(AddConnection);
    }

    private async Task AddConnection()
    {
        bool connectionSuccessful = await TestConnection();
        if (connectionSuccessful)
        {
            await SkEditorAPI.Windows.ShowMessage("Added", $"Successfully added connection to {Host}:{Port}.");
        }
        else
        {
            InfoBarMessage = "Cannot add connection. Please fix the connection issues first.";
            InfoBarSeverity = InfoBarSeverity.Error;
        }
    }

    private async Task<bool> TestConnection()
    {
        string host = Host;
        int? port = int.TryParse(Port, out int p) ? p : null;
        string username = Username;
        string password = Password;

        if (port == null)
        {
            InfoBarMessage = "Port must be a valid number.";
            InfoBarSeverity = InfoBarSeverity.Error;
            return false;
        }

        using var client = new SftpClient(host, port.Value, username, password);

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
            InfoBarMessage = $"Error connecting to {host}:{port}: {ex.Message}";
            InfoBarSeverity = InfoBarSeverity.Error;
            return false;
        }
    }
}