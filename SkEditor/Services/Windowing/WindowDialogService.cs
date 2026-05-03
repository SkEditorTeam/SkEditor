using System;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Tabs;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class WindowDialogService(
    WindowRegistry windowRegistry,
    INotificationService notifications,
    Func<PublishWindow> publishWindowFactory,
    ITabService tabs) : IWindowDialogService
{
    public async Task ShowPublishDialogAsync()
    {
        if (windowRegistry.Active is not { } mainWindow)
        {
            return;
        }

        if (tabs.SelectedTab is not FileTab)
        {
            notifications.ShowError("No file", "You must have a file open to publish.");
            return;
        }

        PublishWindow publishWindow = publishWindowFactory();
        await publishWindow.ShowDialog(mainWindow);
    }
}