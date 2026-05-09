using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Models.Files;
using SkEditor.Models.Messages;

namespace SkEditor.Models.Projects.Local;

public partial class LocalFileNode(string name, string path) : BaseFileNode(name, path)
{
    public override IRelayCommand OpenCommand => OpenFileCommand;

    [RelayCommand]
    public void OpenFile()
    {
        WeakReferenceMessenger.Default.Send(new OpenFileMessage(new LocalFile(Path, Name)));
    }

    public override IEnumerable<ProjectAction> GetSupportedActions() =>
    [
        ProjectAction.Open,
        ProjectAction.Separator,
        ProjectAction.Duplicate,
        ProjectAction.Rename,
        ProjectAction.Delete,
        ProjectAction.Separator,
        ProjectAction.CopyPath,
        ProjectAction.CopyRelativePath,
        ProjectAction.Separator,
        ProjectAction.ShowInExplorer
    ];
}