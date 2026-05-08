using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using SkEditor.Models.Files;

namespace SkEditor.Models.Projects.Local;

public partial class LocalFileNode(string name, string path) : BaseFileNode(name, path)
{
    public override IRelayCommand OpenCommand => OpenFileCommand;

    [RelayCommand]
    public void OpenFile()
    {
        WeakReferenceMessenger.Default.Send(new OpenFileMessage(new LocalFile(Path, Name)));
    }

    public override List<ContextMenuItem> GetMenuItems()
    {
        return
        [
            ContextMenuItem.Item("Open").WithIcon(Symbol.Open).WithCommand(OpenFileCommand),
            ContextMenuItem.Separator,
            .. GetCommonMenuItems(),
            ContextMenuItem.Separator,
            ContextMenuItem.Item("Show in Explorer").WithIcon(Symbol.FolderOpen)
        ];
    }
}