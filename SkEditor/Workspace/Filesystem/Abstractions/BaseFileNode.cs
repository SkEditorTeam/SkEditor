using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using SkEditor.Core.Localization;
using SkEditor.Core.Notifications;
using SkEditor.Core.Notifications.Messages;

namespace SkEditor.Workspace.Filesystem.Abstractions;

public abstract class BaseFileNode(string name, string path) : FileSystemNode(name, path, false)
{
    public override Task LoadChildrenAsync(bool force = false)
    {
        return Task.CompletedTask;
    }

    public override async Task RenameAsync(string newName)
    {
        if (newName.IndexOfAny(InvalidChars) >= 0)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                Notification
                    .Create(TranslationKey.ProjectRenameFailedTitle)
                    .WithMessage(TranslationKey.ProjectRenameErrorNameInvalid)
                    .WithType(NotificationType.Error)));

            return;
        }

        string directory = GetDirectoryName(Path);
        string newPath = CombinePath(directory, newName);

        bool alreadyExists = await AlreadyExistsPredicate(newPath);

        if (alreadyExists)
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

        UpdateName(newName);
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

    public override void UpdateName(string newName)
    {
        Name = newName;

        string directory = GetDirectoryName(Path);
        string newPath = CombinePath(directory, newName);
        UpdatePath(newPath);
    }

    public override void UpdatePath(string newPath)
    {
        Path = newPath;
    }
}