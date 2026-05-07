using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using SkEditor.Models.Files;

namespace SkEditor.Models.Projects.Local;

public partial class LocalFileNode(string name, string path) : BaseFileNode(name, path)
{
    public override IRelayCommand OpenCommand => OpenFileCommand;

    protected override IEnumerable<ContextMenuItem> GetRawContextMenuItems()
    {
        yield return new ContextMenuItem { Header = "Hello", Icon = Symbol.Open, Command = OpenFileCommand };
    }

    [RelayCommand]
    public void OpenFile()
    {
        WeakReferenceMessenger.Default.Send(new OpenFileMessage(new LocalFile(Path, Name)));
    }
}