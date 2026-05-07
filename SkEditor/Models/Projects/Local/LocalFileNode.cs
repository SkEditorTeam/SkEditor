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
        yield return new ContextMenuItem { Header = "Open", Icon = Symbol.Open, Command = OpenFileCommand };
        yield return ContextMenuItem.Separator;
        yield return new ContextMenuItem { Header = "Duplicate", Icon = Symbol.DocumentCopy };
        yield return new ContextMenuItem { Header = "Rename", Icon = Symbol.Rename };
        yield return new ContextMenuItem { Header = "Delete", Icon = Symbol.Delete };
        yield return ContextMenuItem.Separator;
        yield return new ContextMenuItem { Header = "Copy Path", Icon = Symbol.Clipboard };
        yield return new ContextMenuItem { Header = "Copy Relative Path", Icon = Symbol.ClipboardLink };
        yield return new ContextMenuItem { Header = "Show in Explorer", Icon = Symbol.FolderOpen };
    }

    [RelayCommand]
    public void OpenFile()
    {
        WeakReferenceMessenger.Default.Send(new OpenFileMessage(new LocalFile(Path, Name)));
    }
}