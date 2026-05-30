using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Models.Dialogs;
using SkEditor.Models.Remote;
using SkEditor.Utilities;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.ViewModels.Windows;

public partial class RemoteConnectionViewModel(
    IRemoteConnectionService connections,
    IClipboardService clipboard,
    ConnectionsConfig connectionsConfig,
    IDialogService dialogs,
    ITranslationService translation) : ObservableObject
{
    [ObservableProperty]
    public partial RemoteConnectionInfo ConnectionInfo { get; set; } = new();

    [ObservableProperty]
    public partial string? InfoBarMessage { get; set; }

    [ObservableProperty]
    public partial FAInfoBarSeverity InfoBarSeverity { get; set; } = FAInfoBarSeverity.Informational;

    public ObservableCollection<RemoteConnection> Connections { get; } = connectionsConfig.Connections;

    [ObservableProperty]
    public partial RemoteConnection? SelectedConnection { get; set; }

    public IReadOnlyList<RemoteAuthMethod> AuthMethods { get; } =
        Enum.GetValues<RemoteAuthMethod>().ToList();

    public Func<Task<string?>>? RequestKeyFilePicker { get; set; }

    [RelayCommand]
    private async Task SaveConnection()
    {
        if (SelectedConnection is null)
        {
            return;
        }

        RemoteConnectionTestResult result = await connections.TestConnectionAsync(ConnectionInfo);
        if (!result.IsSuccess)
        {
            InfoBarMessage = translation.Get(TranslationKey.RemoteConnectionCannotAddConnection);
            InfoBarSeverity = FAInfoBarSeverity.Error;
            return;
        }

        SelectedConnection.Name = ConnectionInfo.Name;
        SelectedConnection.Host = ConnectionInfo.Host;
        SelectedConnection.Port = int.TryParse(ConnectionInfo.Port, out int port) ? port : 22;
        SelectedConnection.Username = ConnectionInfo.Username;
        SelectedConnection.AuthMethod = ConnectionInfo.AuthMethod;
        SelectedConnection.KeyFilePath = ConnectionInfo.KeyFilePath;

        if (!SelectedConnection.IsAdded)
        {
            connectionsConfig.Connections.Add(SelectedConnection);
            SelectedConnection.IsAdded = true;
        }

        if (ConnectionInfo.IsPasswordAuth)
        {
            if (!string.IsNullOrWhiteSpace(ConnectionInfo.Password))
            {
                connections.SavePassword(SelectedConnection, ConnectionInfo.Password);
                ConnectionInfo.PasswordHash = SelectedConnection.PasswordHash;
                ConnectionInfo.IsPasswordSaved = true;
                ConnectionInfo.Password = string.Empty;
            }
        }
        else
        {
            SelectedConnection.PasswordHash = string.Empty;
        }

        _ = connectionsConfig.Save();

        string hostPort =
            $"{ConnectionInfo.Host}:{(!string.IsNullOrWhiteSpace(ConnectionInfo.Port) ? ConnectionInfo.Port : "22")}";

        InfoBarMessage = translation.Get(TranslationKey.RemoteConnectionAddSuccessMessage, hostPort);
        InfoBarSeverity = FAInfoBarSeverity.Success;
    }

    [RelayCommand]
    private async Task<bool> TestConnection()
    {
        RemoteConnectionTestResult result = await connections.TestConnectionAsync(ConnectionInfo);

        InfoBarMessage = result.Message;
        InfoBarSeverity = result.IsSuccess ? FAInfoBarSeverity.Success : FAInfoBarSeverity.Error;

        return result.IsSuccess;
    }

    [RelayCommand]
    private void AddConnection()
    {
        Clear();
        SelectedConnection = new RemoteConnection
        {
            IsAdded = false
        };
    }

    [RelayCommand]
    private async Task DeleteConnection()
    {
        if (SelectedConnection is null)
        {
            return;
        }

        Dialog dialog = Dialog
            .Create(TranslationKey.RemoteConnectionDeleteTitle)
            .WithMessage(new TranslatableString(TranslationKey.RemoteConnectionDeleteMessage, SelectedConnection.Name))
            .WithIcon(Symbol.Delete)
            .WithButtons(new DialogButton(TranslationKey.ButtonDelete, DialogResult.Ok), DialogButton.Cancel);

        DialogResult result = await dialogs.ShowDialogAsync(dialog);
        if (result != DialogResult.Ok)
        {
            return;
        }

        Connections.Remove(SelectedConnection);
        connectionsConfig.Connections.Remove(SelectedConnection);
        await connectionsConfig.Save();

        Clear();
    }

    [RelayCommand]
    private async Task BrowseKey()
    {
        if (RequestKeyFilePicker == null)
        {
            return;
        }

        string? path = await RequestKeyFilePicker.Invoke();

        if (!string.IsNullOrEmpty(path))
        {
            ConnectionInfo.KeyFilePath = path;
        }
    }

    public event Action? PasteCompleted;

    [RelayCommand]
    private async Task HandlePaste(RoutedEventArgs e)
    {
        string? pasted = await clipboard.GetTextAsync();
        if (string.IsNullOrWhiteSpace(pasted))
        {
            return;
        }

        if (!SftpConnectionStringParser.TryParse(pasted, out SftpConnectionInfo? connectionInfo))
        {
            return;
        }

        e.Handled = true;

        ConnectionInfo = new RemoteConnectionInfo
        {
            Host = connectionInfo.Host,
            Port = connectionInfo.Port.ToString(),
            Username = connectionInfo.Username ?? "",
            Password = connectionInfo.Password ?? "",
            AuthMethod = connectionInfo.Password != null
                ? RemoteAuthMethod.Password
                : ConnectionInfo.AuthMethod
        };

        PasteCompleted?.Invoke();
    }

    partial void OnSelectedConnectionChanged(RemoteConnection? value)
    {
        if (value == null)
        {
            return;
        }

        Clear();

        ConnectionInfo = new RemoteConnectionInfo
        {
            Name = value.Name,
            Host = value.Host,
            Port = value.Port.ToString(),
            Username = value.Username,
            AuthMethod = value.AuthMethod,
            KeyFilePath = value.AuthMethod == RemoteAuthMethod.PrivateKey ? value.KeyFilePath : "",
            PasswordHash = value.PasswordHash,
            IsPasswordSaved = value.AuthMethod == RemoteAuthMethod.Password &&
                              !string.IsNullOrWhiteSpace(value.PasswordHash)
        };
    }

    private void Clear()
    {
        ConnectionInfo = new RemoteConnectionInfo();
        InfoBarMessage = null;
        InfoBarSeverity = FAInfoBarSeverity.Informational;
    }
}