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
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Windows;

public partial class RemoteConnectionViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    private string _name = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    private string _host = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    private string _port = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    private string _username = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    private string _password = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RequiredFieldsFilled), nameof(ConnectionFieldsFilled))]
    private string _keyFilePath = "";

    [ObservableProperty]
    private string? _infoBarMessage;

    [ObservableProperty]
    private InfoBarSeverity _infoBarSeverity = InfoBarSeverity.Informational;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(RequiredFieldsFilled),
        nameof(ConnectionFieldsFilled),
        nameof(IsPasswordAuth),
        nameof(IsPrivateKeyAuth))]
    private AuthMethod _authMethod = AuthMethod.Password;

    public IReadOnlyList<AuthMethod> AuthMethods { get; } =
        Enum.GetValues<AuthMethod>().ToList();

    public bool IsPasswordAuth => AuthMethod == AuthMethod.Password;
    public bool IsPrivateKeyAuth => AuthMethod == AuthMethod.PrivateKey;

    public bool ConnectionFieldsFilled =>
        !string.IsNullOrWhiteSpace(Host) &&
        !string.IsNullOrWhiteSpace(Username) &&
        (IsPasswordAuth
            ? !string.IsNullOrWhiteSpace(Password)
            : !string.IsNullOrWhiteSpace(KeyFilePath));

    public bool RequiredFieldsFilled =>
        !string.IsNullOrWhiteSpace(Name) &&
        ConnectionFieldsFilled;

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
            string hostPort = $"{Host}:{(!string.IsNullOrWhiteSpace(Port) ? Port : "22")}";

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
        string host = Host;
        int port = int.TryParse(Port, out int p) ? p : 22;
        string username = Username;
        string password = Password;


        bool invalidKeyFile = IsPrivateKeyAuth && (string.IsNullOrWhiteSpace(KeyFilePath) || !File.Exists(KeyFilePath));
        if (invalidKeyFile)
        {
            InfoBarMessage = Translation.Get(TranslationKey.RemoteConnectionInvalidKeyPath);
            InfoBarSeverity = InfoBarSeverity.Error;
            return false;
        }

        try
        {
            AuthenticationMethod authMethod = IsPasswordAuth
                ? new PasswordAuthenticationMethod(username, password)
                : new PrivateKeyAuthenticationMethod(username, new PrivateKeyFile(KeyFilePath));

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
            KeyFilePath = path;
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

public enum AuthMethod
{
    Password,
    PrivateKey
}