using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using SkEditor.Models.Messages;
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
                TranslationKey.ProjectRenameFailedTitle,
                TranslationKey.ProjectRenameErrorNameInvalid, NotificationType.Error));

            return;
        }

        string newPath = System.IO.Path.Combine(directory, newName);

        if (AlreadyExistsPredicate(newPath).Result)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslationKey.ProjectRenameFailedTitle,
                TranslationKey.ProjectErrorNameExists, NotificationType.Error));

            return;
        }

        try
        {
            await RenameFileAction(Path, newPath);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                TranslationKey.ProjectRenameFailedTitle, TranslatableString.FromRawString(ex.Message),
                NotificationType.Error));

            Log.Error(ex, "Failed to rename file {Path} to {NewName}", Path, newName);
            return;
        }

        Path = newPath;
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
                TranslatableString.FromRawString("File Delete Failed"),
                TranslatableString.FromRawString(ex.Message),
                NotificationType.Error));

            Log.Error(ex, "Failed to delete file {Path}", Path);
            return false;
        }
    }
}