using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Workspace.Files.Messages;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Filesystem.Abstractions;
using SkEditor.Workspace.Projects.Models;

namespace SkEditor.Workspace.Filesystem.Local;

public partial class LocalFileNode(string name, string path) : BaseFileNode(name, path)
{
    public override char[] InvalidChars => LocalFileSystem.InvalidChars;

    public override IRelayCommand OpenCommand => OpenFileCommand;

    protected override Func<string, Task<bool>> AlreadyExistsPredicate => LocalFileSystem.AlreadyExistsPredicate;

    protected override Func<string, Task> DeleteAction => async path => await Task.Run(() => File.Delete(path));

    protected override Func<string, string, Task> RenameFileAction =>
        async (oldPath, newPath) => await Task.Run(() => File.Move(oldPath, newPath));

    [RelayCommand]
    public void OpenFile()
    {
        WeakReferenceMessenger.Default.Send(new OpenFileMessage(new LocalFile(Path, Name)));
    }

    public override void UpdateName(string newName)
    {
        string oldPath = Path;
        base.UpdateName(newName);

        WeakReferenceMessenger.Default.Send(new UpdateFileDetailsMessage(oldPath, Path, Name));
    }

    public override void UpdatePath(string newPath)
    {
        string oldPath = Path;
        base.UpdatePath(newPath);

        WeakReferenceMessenger.Default.Send(new UpdateFileDetailsMessage(oldPath, Path, Name));
    }

    public override IEnumerable<ProjectAction> GetSupportedActions()
    {
        return
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
}