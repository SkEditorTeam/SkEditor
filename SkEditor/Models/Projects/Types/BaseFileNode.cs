using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using SkEditor.Models.Messages;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Projects.Types;

public abstract class BaseFileNode(string name, string path) : FileSystemNode(name, path, false)
{
    public override Task LoadChildrenAsync()
    {
        return Task.CompletedTask;
    }

    public override async Task RenameAsync(string newName)
    {
        if (System.IO.Path.GetDirectoryName(Path) is not { } directory)
        {
            return;
        }

        if (newName.IndexOfAny(InvalidChars) >= 0)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.ProjectRenameFailedTitle)
                    .WithMessage(TranslationKey.ProjectRenameErrorNameInvalid)
                    .WithType(NotificationType.Error)));

            return;
        }

        string newPath = System.IO.Path.Combine(directory, newName);

        if (AlreadyExistsPredicate(newPath).Result)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.ProjectRenameFailedTitle)
                    .WithMessage(TranslationKey.ProjectErrorNameExists)
                    .WithType(NotificationType.Error)));

            return;
        }

        try
        {
            await RenameFileAction(Path, newPath);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.ProjectRenameFailedTitle)
                    .WithMessage(TranslatableString.FromRawString(ex.Message))
                    .WithType(NotificationType.Error)));

            Log.Error(ex, "Failed to rename file {Path} to {NewName}", Path, newName);
            return;
        }

        UpdatePath(newPath);
        Name = newName;
    }

    public override async Task<bool> DeleteAsync()
    {
        try
        {
            await DeleteAction(Path);
            return true;
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.FileExplorerDeleteFailedTitle)
                    .WithMessage(TranslatableString.FromRawString(ex.Message))
                    .WithType(NotificationType.Error)));

            Log.Error(ex, "Failed to delete file {Path}", Path);
            return false;
        }
    }

    public override void UpdatePath(string newPath)
    {
        Path = newPath;
    }
}