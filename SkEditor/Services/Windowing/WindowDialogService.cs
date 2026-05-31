using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Localization;
using SkEditor.ViewModels.Tabs;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class WindowDialogService(
    WindowRegistry windowRegistry,
    INotificationService notifications,
    Func<PublishWindow> publishWindowFactory,
    Func<GoToLineWindow> goToLineWindowFactory,
    Func<AboutWindow> aboutWindowFactory,
    Func<ManageRemoteConnectionsWindow> manageRemoteConnectionsWindowFactory,
    ITabService tabs) : IWindowDialogService
{
    public async Task ShowPublishDialogAsync()
    {
        await ShowDialogAsync(publishWindowFactory, true);
    }

    public async Task ShowGoToLineDialogAsync()
    {
        await ShowDialogAsync(goToLineWindowFactory, true);
    }

    public async Task ShowAboutDialogAsync()
    {
        await ShowDialogAsync(aboutWindowFactory);
    }

    public async Task ShowManageRemoteConnectionsDialogAsync()
    {
        await ShowDialogAsync(manageRemoteConnectionsWindowFactory);
    }

    private async Task<TResult?> ShowDialogAsync<TWindow, TResult>(Func<TWindow> windowFactory,
        bool requireFile = false)
        where TWindow : Window
        where TResult : class
    {
        if (windowRegistry.Active is not { } mainWindow)
        {
            return null;
        }

        if (requireFile && tabs.SelectedTab is not FileTabViewModel)
        {
            Notification notification = Notification
                .Create(TranslationKey.NoFileNotificationTitle)
                .WithMessage(TranslationKey.NoFileNotificationMessage)
                .WithType(NotificationType.Error);

            notifications.Show(notification);
            return null;
        }

        TWindow window = windowFactory();

        try
        {
            return await window.ShowDialog<TResult>(mainWindow);
        }
        finally
        {
            if (tabs.SelectedTab is FileTabViewModel fileTab)
            {
                fileTab.RequestFocus();
            }
        }
    }

    private async Task ShowDialogAsync<TWindow>(Func<TWindow> windowFactory, bool requireFile = false)
        where TWindow : Window
    {
        await ShowDialogAsync<TWindow, object>(windowFactory, requireFile);
    }
}