using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using SkEditor.Models.Files;
using SkEditor.Models.Messages;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Projects.Local;

public partial class LocalFileNode(string name, string path) : BaseFileNode(name, path)
{
    public override IRelayCommand OpenCommand => OpenFileCommand;

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

    public override Task Rename(string newName)
    {
        if (System.IO.Path.GetDirectoryName(Path) is not { } directory) return Task.CompletedTask;

        string newPath;
        try
        {
            newPath = System.IO.Path.Combine(directory, newName);
            System.IO.File.Move(Path, newPath);
        }
        catch (System.Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslatableString.FromRawString("Rename failed"), TranslatableString.FromRawString(ex.Message), NotificationType.Error));
            
            Log.Error(ex, "Failed to rename file {Path} to {NewName}", Path, newName);
            return Task.CompletedTask;
        }
        
        Name = newName;
        Path = newPath;

        return Task.CompletedTask;
    }
}