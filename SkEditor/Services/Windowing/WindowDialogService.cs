using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class WindowDialogService(
    WindowRegistry windowRegistry,
    INotificationService notifications,
    Func<PublishWindow> publishWindowFactory,
    Func<GoToLineWindow> goToLineWindowFactory,
    ITabService tabs) : IWindowDialogService
{
    public async Task ShowPublishDialogAsync()
    {
        await ShowDialogAsync(publishWindowFactory);
    }

    private async Task ShowDialogAsync<T>(Func<T> windowFactory) where T : Window
    {
        if (windowRegistry.Active is not { } mainWindow)
        {
            return;
        }

        if (tabs.SelectedTab is not FileTab)
        {
            notifications.ShowError(TranslationKey.NoFileNotificationTitle, TranslationKey.NoFileNotificationMessage);
            return;
        }

        T window = windowFactory();
        await window.ShowDialog(mainWindow);
    }
}