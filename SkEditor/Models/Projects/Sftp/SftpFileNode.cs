using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
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

    public override List<ContextMenuItem> GetMenuItems()
    {
        return
        [
            ContextMenuItem.Item("Open").WithIcon(Symbol.Open).WithCommand(OpenFileCommand),
            ContextMenuItem.Item("Download").WithIcon(Symbol.ArrowDownload),
            ContextMenuItem.Separator,
            .. GetCommonMenuItems()
        ];
    }
}