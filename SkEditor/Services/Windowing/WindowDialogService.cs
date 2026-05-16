using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Notifications;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class WindowDialogService(
    WindowRegistry windowRegistry,
    INotificationService notifications,
    Func<PublishWindow> publishWindowFactory,
    Func<GoToLineWindow> goToLineWindowFactory,
    Func<AboutWindow> aboutWindowFactory,
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

    private async Task ShowDialogAsync<T>(Func<T> windowFactory, bool requireFile = false) where T : Window
    {
        if (windowRegistry.Active is not { } mainWindow)
        {
            return;
        }

        if (requireFile && tabs.SelectedTab is not FileTab)
        {
            Notification notification = Notification
                .Create(TranslationKey.NoFileNotificationTitle)
                .WithMessage(TranslationKey.NoFileNotificationMessage)
                .WithType(NotificationType.Error);

            notifications.Show(notification);
            return;
        }

        T window = windowFactory();
        await window.ShowDialog(mainWindow);
    }
}