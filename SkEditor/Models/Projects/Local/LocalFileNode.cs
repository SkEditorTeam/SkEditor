using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using SkEditor.Models.Files;
using SkEditor.Models.Messages;
using SkEditor.Models.Projects.Types;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Projects.Local;

public partial class LocalFileNode(string name, string path) : BaseFileNode(name, path)
{
    public static readonly char[] InvalidChars = System.IO.Path.GetInvalidFileNameChars();

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

    public override Task RenameAsync(string newName)
    {
        if (System.IO.Path.GetDirectoryName(Path) is not { } directory)
        {
            return Task.CompletedTask;
        }

        if (newName.IndexOfAny(InvalidChars) >= 0)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslationKey.ProjectRenameFailedTitle,
                TranslationKey.ProjectRenameErrorNameInvalid, NotificationType.Error));

            return Task.CompletedTask;
        }

        string newPath = System.IO.Path.Combine(directory, newName);

        if (File.Exists(newPath))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslationKey.ProjectRenameFailedTitle,
                TranslationKey.ProjectErrorNameExists, NotificationType.Error));

            return Task.CompletedTask;
        }

        try
        {
            File.Move(Path, newPath);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslationKey.ProjectRenameFailedTitle, TranslatableString.FromRawString(ex.Message),
                NotificationType.Error));

            Log.Error(ex, "Failed to rename file {Path} to {NewName}", Path, newName);
            return Task.CompletedTask;
        }

        Path = newPath;
        Name = newName;

        return Task.CompletedTask;
    }

    public override Task<bool> DeleteAsync()
    {
        try
        {
            File.Delete(Path);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslatableString.FromRawString("File Delete Failed"),
                TranslatableString.FromRawString(ex.Message),
                NotificationType.Error));

            Log.Error(ex, "Failed to delete file {Path}", Path);
            return Task.FromResult(false);
        }
    }
}