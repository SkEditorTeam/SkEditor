using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Models.Notifications;
using SkEditor.Models.Remote;
using SkEditor.Utilities;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Windows;

public partial class RemoteConnectionViewModel(
    IRemoteConnectionService connections,
    INotificationService notifications,
    IClipboardService clipboard,
    ITranslationService translation) : ObservableObject
{
    [ObservableProperty]
    public partial RemoteConnectionInfo ConnectionInfo { get; set; } = new();

    [ObservableProperty]
    public partial string? InfoBarMessage { get; set; }

    [ObservableProperty]
    public partial FAInfoBarSeverity InfoBarSeverity { get; set; } = FAInfoBarSeverity.Informational;

    public IReadOnlyList<RemoteAuthMethod> AuthMethods { get; } =
        Enum.GetValues<RemoteAuthMethod>().ToList();

    public Func<Task<string?>>? RequestKeyFilePicker { get; set; }

    public event EventHandler? CloseRequested;

    [RelayCommand]
    private async Task AddConnection()
    {
        RemoteConnectionTestResult result = await connections.TestConnectionAsync(ConnectionInfo);
        if (!result.IsSuccess)
        {
            InfoBarMessage = translation.Get(TranslationKey.RemoteConnectionCannotAddConnection);
            InfoBarSeverity = FAInfoBarSeverity.Error;

            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            string hostPort =
                $"{ConnectionInfo.Host}:{(!string.IsNullOrWhiteSpace(ConnectionInfo.Port) ? ConnectionInfo.Port : "22")}";

            Notification notification = Notification
                .Create(TranslationKey.RemoteConnectionAddSuccessTitle)
                .WithMessage(new TranslatableString(TranslationKey.RemoteConnectionAddSuccessMessage, hostPort))
                .WithType(NotificationType.Success)
                .WithDuration(TimeSpan.FromSeconds(5));

            notifications.Show(notification);

            CloseRequested?.Invoke(this, EventArgs.Empty);
            Clear();
        });
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

        ConnectionInfo.Host = connectionInfo.Host;
        ConnectionInfo.Port = connectionInfo.Port.ToString();
        ConnectionInfo.Username = connectionInfo.Username ?? "";
        ConnectionInfo.Password = connectionInfo.Password ?? "";
        ConnectionInfo.AuthMethod = connectionInfo.Password != null
            ? RemoteAuthMethod.Password
            : ConnectionInfo.AuthMethod;

        PasteCompleted?.Invoke();
    }

    private void Clear()
    {
        ConnectionInfo = new RemoteConnectionInfo();
        InfoBarMessage = null;
        InfoBarSeverity = FAInfoBarSeverity.Informational;
    }
}