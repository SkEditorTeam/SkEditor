using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Renci.SshNet;
using Serilog;
using SkEditor.Models.Files;
using SkEditor.Models.Messages;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Projects.Sftp;

public partial class SftpFileNode(string name, string path, SftpClient client) : BaseFileNode(name, path)
{
    public override IRelayCommand OpenCommand => OpenFileCommand;

    [RelayCommand]
    public void OpenFile()
    {
        WeakReferenceMessenger.Default.Send(new OpenFileMessage(new SftpFile(Path, Name, client)));
    }

    public override IEnumerable<ProjectAction> GetSupportedActions() =>
    [
        ProjectAction.Open,
        ProjectAction.Separator,
        ProjectAction.Download,
        ProjectAction.Separator,
        ProjectAction.Duplicate,
        ProjectAction.Rename,
        ProjectAction.Delete,
        ProjectAction.Separator,
        ProjectAction.CopyPath,
        ProjectAction.CopyRelativePath
    ];

    public override async Task Rename(string newName)
    {
        char[] invalidChars = [ '/', '\\', '\0' ];
        if (newName.IndexOfAny(invalidChars) >= 0)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslationKey.ProjectRenameFailedTitle,
                TranslationKey.ProjectRenameErrorNameInvalid, NotificationType.Error));

            return;
        }
        
        string newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Path)!, newName);

        if (await client.ExistsAsync(newPath, CancellationToken.None))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslationKey.ProjectRenameFailedTitle,
                TranslationKey.ProjectErrorNameExists, NotificationType.Error));

            return;
        }

        try
        {
            await client.RenameFileAsync(Path, newPath, CancellationToken.None);
        }
        catch (System.Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslatableString.FromRawString("Rename failed"), TranslatableString.FromRawString(ex.Message), NotificationType.Error));
            
            Log.Error(ex, "Failed to rename file {Path} to {NewName}", Path, newName);
            return;
        }
        
        Path = newPath;
        Name = newName;
    }
}