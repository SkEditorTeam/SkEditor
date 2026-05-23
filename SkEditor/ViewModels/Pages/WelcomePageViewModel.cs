using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Files;
using SkEditor.Models.Messages;
using SkEditor.Models.Notifications;
using SkEditor.Models.Welcome;
using SkEditor.Utilities;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Pages;

public partial class WelcomePageViewModel(
    IWelcomeService welcome,
    IFileService files,
    IRecentFileService recents,
    IClipboardService clipboard,
    INotificationService notifications) : ObservableObject
{
    public string Version { get; } = $"v{App.GetInformationalVersion()}";

    public ReadOnlyObservableCollection<WelcomeScreenRecentItem> RecentItems => welcome.RecentItems;
    public ObservableCollection<WelcomeScreenSection> WelcomeSections => welcome.WelcomeSections;

    [RelayCommand]
    public async Task OpenRecent(WelcomeScreenRecentItem item)
    {
        if (item.IsFolder)
        {
            OpenFolder(item);
        }
        else
        {
            await OpenFile(item);
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
        if (item.IsFolder)
        {
            LinkOpener.OpenFolder(item.Path);
        }
        else
        {
            LinkOpener.OpenFolderWithFileSelection(item.Path);
        }
    }

    [RelayCommand]
    public void Remove(WelcomeScreenRecentItem item)
    {
        recents.RemoveRecentItem(item.Path);
    }

    private async Task OpenFile(WelcomeScreenRecentItem item)
    {
        if (!File.Exists(item.Path))
        {
            Notification notif = Notification
                .Create(TranslationKey.FileExplorerFileNotFoundTitle)
                .WithMessage(new TranslatableString(TranslationKey.FileExplorerFileNotFoundMessage, item.Name))
                .WithDuration(TimeSpan.FromSeconds(10))
                .WithType(NotificationType.Error);

            notifications.Show(notif);

            recents.RemoveRecentItem(item.Path);
            return;
        }

        await files.OpenFileAsync(new LocalFile(item.Path, item.Name));
    }

    private void OpenFolder(WelcomeScreenRecentItem item)
    {
        if (!Directory.Exists(item.Path))
        {
            Notification notif = Notification
                .Create(TranslationKey.FileExplorerFolderNotFoundTitle)
                .WithMessage(new TranslatableString(TranslationKey.FileExplorerFolderNotFoundMessage, item.Name))
                .WithDuration(TimeSpan.FromSeconds(10))
                .WithType(NotificationType.Error);

            notifications.Show(notif);

            recents.RemoveRecentItem(item.Path);
            return;
        }

        WeakReferenceMessenger.Default.Send(new OpenExplorerFolderMessage(item.Path));
    }
}