using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Messages;
using SkEditor.Models.Notifications;
using SkEditor.Models.Projects.Local;
using SkEditor.Utilities.Localization;

namespace SkEditor.Services.Files;

public class ProjectService(
    IFilePickerService filePicker,
    INotificationService notifs,
    IRecentFileService recents,
    IMessenger messenger) : IProjectService
{
    public LocalFileSystem? CurrentProject { get; private set; }
    public string? CurrentProjectPath => CurrentProject?.Root?.Path;

    public void OpenProject(string path)
    {
        LocalFileSystem localFileSystem = new(path);
        CurrentProject = localFileSystem;

        string? name = Path.GetFileName(Path.GetDirectoryName(path));
        if (name is not null)
        {
            recents.AddRecentItem(name, path, true, DateTime.Now);
        }

        messenger.Send(new ProjectChangedMessage(localFileSystem));
    }

    public async Task<bool> OpenProjectAsync()
    {
        string? path = (await filePicker.PickFoldersAsync(false))?.FirstOrDefault();
        if (path is null)
        {
            return false;
        }

        if (!Directory.Exists(path))
        {
            Notification notification = Notification
                .Create(TranslationKey.FileExplorerFolderDoesntExistTitle)
                .WithMessage(TranslationKey.FileExplorerFolderDoesntExistMessage)
                .WithType(NotificationType.Error);

            notifs.Show(notification);
            return false;
        }

        OpenProject(path);
        return true;
    }
}