using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Core.Localization;
using SkEditor.Core.Notifications;
using SkEditor.Core.System;
using SkEditor.Welcome.Models;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Projects.Messages;
using SkEditor.Workspace.Tabs;

namespace SkEditor.Welcome.UI;

public partial class WelcomePageViewModel(
    IWelcomeService welcome,
    IFileTabService fileTabs,
    IRecentFileService recents,
    IClipboardService clipboard,
    INotificationService notifications,
    ILinkService links) : ObservableObject
{
    public ReadOnlyObservableCollection<WelcomeScreenRecentItem> RecentItems => welcome.RecentItems;
    public string Version { get; } = $"v{App.GetInformationalVersion()}";
    public ObservableCollection<WelcomeScreenSection> WelcomeSections => welcome.WelcomeSections;

    [RelayCommand]
    public async Task OpenRecent(WelcomeScreenRecentItem item)
    {
        if (!CheckItemExists(item))
        {
            return;
        }
        
        if (item.IsFolder)
        {
            WeakReferenceMessenger.Default.Send(new OpenExplorerFolderMessage(item.Path));
        }
        else
        {
            await fileTabs.OpenFileAsync(new LocalFile(item.Path, item.Name));
        }
    }

    [RelayCommand]
    public async Task CopyPath(WelcomeScreenRecentItem item)
    {
        await clipboard.CopyTextAsync(item.Path);
    }

    [RelayCommand]
    public void ShowInExplorer(WelcomeScreenRecentItem item)
    {
        if (!CheckItemExists(item))
        {
            return;
        }
        
        if (item.IsFolder)
        {
            links.OpenFolder(item.Path);
        }
        else
        {
            links.OpenFolderWithFileSelection(item.Path);
        }
    }

    [RelayCommand]
    public void Remove(WelcomeScreenRecentItem item)
    {
        recents.RemoveRecentItem(item.Path);
    }

    private bool CheckItemExists(WelcomeScreenRecentItem item)
    {
        if (item.IsFolder)
        {
            if (Directory.Exists(item.Path))
            {
                return true;
            }

            Notification notif = Notification
                .Create(TranslationKey.FileExplorerFolderNotFoundTitle)
                .WithMessage(new TranslatableString(TranslationKey.FileExplorerFolderNotFoundMessage, item.Name))
                .WithDuration(TimeSpan.FromSeconds(10))
                .WithType(NotificationType.Error);

            notifications.Show(notif);
        }
        else
        {
            if (File.Exists(item.Path))
            {
                return true;
            }

            Notification notif = Notification
                .Create(TranslationKey.FileExplorerFileNotFoundTitle)
                .WithMessage(new TranslatableString(TranslationKey.FileExplorerFileNotFoundMessage, item.Name))
                .WithDuration(TimeSpan.FromSeconds(10))
                .WithType(NotificationType.Error);

            notifications.Show(notif);
        }

        recents.RemoveRecentItem(item.Path);
        return false;
    }
}