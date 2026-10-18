using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Workspace.Files.Messages;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Filesystem.Abstractions;
using SkEditor.Workspace.Projects.Models;
using SkEditor.Workspace.Remote;
using SkEditor.Workspace.Remote.Models;

namespace SkEditor.Workspace.Filesystem.Sftp;

public partial class SftpFileNode(
    string name,
    string path,
    RemoteConnection connection,
    IRemoteConnectionService service)
    : BaseFileNode(name, path)
{
    public override char[] InvalidChars => SftpFileSystem.InvalidChars;

    public override IRelayCommand OpenCommand => OpenFileCommand;

    protected override Func<string, Task<bool>> AlreadyExistsPredicate =>
        SftpFileSystem.GetAlreadyExistsPredicate(service, connection);

    protected override Func<string, Task> DeleteAction =>
        async path =>
            await service.ExecuteSafeAsync(connection,
                async c => await c.DeleteFileAsync(path, CancellationToken.None));

    protected override Func<string, string, Task> RenameFileAction => async (oldPath, newPath) =>
    {
        await service.ExecuteSafeAsync(connection,
            async c => await c.RenameFileAsync(oldPath, newPath, CancellationToken.None));
    };

    [RelayCommand]
    public void OpenFile()
    {
        WeakReferenceMessenger.Default.Send(new OpenFileMessage(new SftpFile(Path, Name, connection, service)));
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

    protected override string CombinePath(string folderPath, string itemName)
    {
        return $"{folderPath.TrimEnd('/')}/{itemName}";
    }

    protected override string GetDirectoryName(string path)
    {
        int lastSlash = path.LastIndexOf('/');
        return lastSlash <= 0 ? "/" : path[..lastSlash];
    }
}