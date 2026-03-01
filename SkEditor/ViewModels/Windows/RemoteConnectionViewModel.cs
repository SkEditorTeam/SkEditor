using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Contracts.Services;
using SkEditor.Models.Remote;
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
        RemoteConnectionTestResult result = await connectionService.TestConnection(ConnectionInfo);
        if (!result.IsSuccess)
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
        RemoteConnectionTestResult result = await connectionService.TestConnection(ConnectionInfo);
        
        InfoBarMessage = result.Message;
        InfoBarSeverity = result.IsSuccess ? InfoBarSeverity.Success : InfoBarSeverity.Error;
        
        return result.IsSuccess;
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