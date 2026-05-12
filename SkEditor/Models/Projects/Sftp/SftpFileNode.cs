using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Renci.SshNet;
using SkEditor.Models.Files;
using SkEditor.Models.Messages;
using SkEditor.Models.Projects.Types;

namespace SkEditor.Models.Projects.Sftp;

public partial class SftpFileNode(string name, string path, SftpClient client) : BaseFileNode(name, path)
{
    protected override Func<string, string, Task> RenameFileAction => async (oldPath, newPath) =>
        await client.RenameFileAsync(oldPath, newPath, CancellationToken.None);

    public override char[] InvalidChars { get; } = ['/', '\\', '\0'];

    public override IRelayCommand OpenCommand => OpenFileCommand;

    protected override Func<string, Task<bool>> AlreadyExistsPredicate =>
        async path => await client.ExistsAsync(path, CancellationToken.None);

    protected override Func<string, Task> DeleteAction =>
        async path => await client.DeleteFileAsync(path, CancellationToken.None);

    [RelayCommand]
    public void OpenFile()
    {
        WeakReferenceMessenger.Default.Send(new OpenFileMessage(new SftpFile(Path, Name, client)));
    }

    public override IEnumerable<ProjectAction> GetSupportedActions()
    {
        return
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
    }
}