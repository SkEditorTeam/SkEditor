using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using Renci.SshNet;
using SkEditor.Models.Files;

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
            new ContextMenuItem { Header = "Open", Icon = Symbol.Open, Command = OpenFileCommand },
            new ContextMenuItem { Header = "Download", Icon = Symbol.ArrowDownload },
            ContextMenuItem.Separator,
            new ContextMenuItem { Header = "Duplicate", Icon = Symbol.DocumentCopy },
            new ContextMenuItem { Header = "Rename", Icon = Symbol.Rename },
            new ContextMenuItem { Header = "Delete", Icon = Symbol.Delete },
            ContextMenuItem.Separator,
            new ContextMenuItem { Header = "Copy Path", Icon = Symbol.Clipboard },
            new ContextMenuItem { Header = "Copy Relative Path", Icon = Symbol.ClipboardLink }
        ];
    }
}