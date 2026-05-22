using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Messages;
using SkEditor.Models.Notifications;
using SkEditor.Models.Projects.Local;
using SkEditor.Models.UI;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls.Sidebar.Panes;

public partial class ExplorerViewModel : ObservableObject, ISidebarPanelViewModel, IRecipient<OpenExplorerFolderMessage>
{
    private readonly IFilePickerService _filePicker;
    private readonly IMessenger _messenger;
    private readonly INotificationService _notifications;
    private readonly IRecentFileService _recents;

    public ExplorerViewModel(FileTreeViewModel fileTree,
        IFilePickerService filePicker,
        IRecentFileService recents,
        INotificationService notifications,
        IMessenger messenger)
    {
        _filePicker = filePicker;
        _recents = recents;
        _notifications = notifications;
        FileTree = fileTree;

        _messenger = messenger;
        _messenger.Register(this);
    }

    public required FileTreeViewModel FileTree { get; init; }

    public void Receive(OpenExplorerFolderMessage message)
    {
        _ = HandleOpenExplorerFolderMessageAsync(message);
    }

    public string Name => "Explorer";
    public StandardIcon Icon => Symbol.Folder;

    [RelayCommand]
    public async Task<bool> OpenFolder()
    {
        string? path = (await _filePicker.PickFoldersAsync(false))?.FirstOrDefault();
        if (path is null)
        {
            return false;
        }

        if (!Directory.Exists(path))
        {
            Notification notification = Notification
                .Create(TranslatableString.FromRawString("Oops!"))
                .WithMessage(TranslatableString.FromRawString("The selected folder does not exist"))
                .WithType(NotificationType.Error);

            _notifications.Show(notification);
            return false;
        }

        await OpenFolder(path);
        return true;
    }

    public async Task OpenFolder(string path)
    {
        LocalFileSystem localFileSystem = new(path);
        FileTree.FileSystem = localFileSystem;

        string? name = Path.GetFileName(Path.GetDirectoryName(path));
        if (name is not null)
        {
            _recents.AddRecentItem(name, path, true, DateTime.Now);
        }

        await FileTree.LoadFileSystemAsync();
    }

    private async Task HandleOpenExplorerFolderMessageAsync(
        OpenExplorerFolderMessage m)
    {
        if (m.FolderPath is not null)
        {
            await OpenFolder(m.FolderPath);
        }
        else
        {
            bool success = await OpenFolder();
            if (!success)
            {
                return;
            }
        }

        _messenger.Send(new RequestPaneOpenMessage(Name));
    }
}