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

    protected override IEnumerable<ContextMenuItem> GetRawContextMenuItems()
    {
        yield return new ContextMenuItem { Header = "Hello3", Icon = Symbol.Open };
    }

    [RelayCommand]
    public void OpenFile()
    {
        WeakReferenceMessenger.Default.Send(new OpenFileMessage(new SftpFile(Path, Name, client)));
    }
}