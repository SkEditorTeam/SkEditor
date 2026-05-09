using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Renci.SshNet;
using SkEditor.Models.Files;
using SkEditor.Models.Messages;

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
}