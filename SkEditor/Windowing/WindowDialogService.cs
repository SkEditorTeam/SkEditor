using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using SkEditor.Core.Localization;
using SkEditor.Core.Notifications;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Editor.UI.Windows;
using SkEditor.Integrations.Publishing.UI;
using SkEditor.Windowing.UI.About;
using SkEditor.Workspace.Remote.UI;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Windowing;

public class WindowDialogService(
    IWindowRegistry windowRegistry,
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

        if (requireFile && tabs.SelectedTab is not ITextEditorTab)
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
            if (tabs.SelectedTab is IFocusableTab focusableTab)
            {
                focusableTab.RequestFocus();
            }
        }
    }

    private async Task ShowDialogAsync<TWindow>(Func<TWindow> windowFactory, bool requireFile = false)
        where TWindow : Window
    {
        await ShowDialogAsync<TWindow, object>(windowFactory, requireFile);
    }
}