using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Models.Notifications;
using SkEditor.Models.Remote;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Windows;

public partial class RemoteConnectionViewModel(
    IRemoteConnectionService connections,
    INotificationService notifications,
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

    private void Clear()
    {
        ConnectionInfo = new RemoteConnectionInfo();
        InfoBarMessage = null;
        InfoBarSeverity = FAInfoBarSeverity.Informational;
    }
}