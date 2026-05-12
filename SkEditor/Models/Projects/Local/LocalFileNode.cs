using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Models.Files;
using SkEditor.Models.Messages;
using SkEditor.Models.Projects.Types;

namespace SkEditor.Models.Projects.Local;

public partial class LocalFileNode(string name, string path) : BaseFileNode(name, path)
{
    protected override Func<string, string, Task> RenameFileAction =>
        async (oldPath, newPath) => await Task.Run(() => File.Move(oldPath, newPath));

    public override char[] InvalidChars { get; } = System.IO.Path.GetInvalidFileNameChars();

    public override IRelayCommand OpenCommand => OpenFileCommand;

    protected override Func<string, Task<bool>> AlreadyExistsPredicate =>
        path => Task.FromResult(File.Exists(path) || Directory.Exists(path));

    protected override Func<string, Task> DeleteAction => async path => await Task.Run(() => File.Delete(path));

    [RelayCommand]
    public void OpenFile()
    {
        WeakReferenceMessenger.Default.Send(new OpenFileMessage(new LocalFile(Path, Name)));
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