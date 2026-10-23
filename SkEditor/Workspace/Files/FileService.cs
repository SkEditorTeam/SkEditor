using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentIcons.Common;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Editor.Features;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Workspace.Files;

public class FileService(
    IFilePickerService filePicker,
    IRecentFileService recentFiles,
    INotificationService notifications,
    IDialogService dialogs,
    IFileFeatureService features,
    ILogService logs)
    : IFileService
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    private CancellationTokenSource? _saveCts;

    /// <summary>
    ///     Prompts the user to select a save location and saves the file there. Updates the tab's header and path accordingly
    /// </summary>
    /// <returns>True if success, false otherwise (cancelled, failed)</returns>
    public async Task<bool> SaveFileAsAsync(IFileTab fileTab)
    {
        string? oldPath = fileTab.File.Path;

        string? path = await filePicker.SaveFileAsync(fileTab.Header.Value, oldPath);
        if (path == null)
        {
            return false;
        }

        fileTab.File.Path = path;
        fileTab.UpdateHeader();

        if (fileTab is IFeatureTab featureTab)
        {
            features.UpdateFeatures(featureTab);
        }
        
        recentFiles.UpdateRecentItem(oldPath, path);

        return await PerformSaveAsync(fileTab);
    }
    
    public async Task<bool> SaveFileAsync(IFileTab fileTab)
    {
        if (fileTab.File.Path == null)
        {
            return await SaveFileAsAsync(fileTab);
        }

        return await PerformSaveAsync(fileTab);
    }
    
    /// <summary>
    ///     Performs the actual save operation
    /// </summary>
    /// <returns>True if success, false otherwise (failed)</returns>
    private async Task<bool> PerformSaveAsync(IFileTab fileTab)
    {
        if (_saveCts is not null)
        {
            await _saveCts.CancelAsync();
        }

        _saveCts = new CancellationTokenSource();
        CancellationToken token = _saveCts.Token;

        using MemoryStream memoryStream = new();
        await using StreamWriter writer = new(memoryStream, Utf8NoBom, leaveOpen: true);

        fileTab.WriteContentStream(writer);

        await writer.FlushAsync(token);
        memoryStream.Position = 0;
        
        try
        {
            await _saveSemaphore.WaitAsync(token);
            await fileTab.File.SaveAsync(memoryStream, token);
            await fileTab.File.UpdateSnapshotAsync(memoryStream);
        }
        catch (UnauthorizedAccessException ex)
        {
            logs.Error(ex, "Failed to save file due to unauthorized access.");
            Notification notification = Notification
                .Create(TranslationKey.FileExplorerSaveFailedTitle)
                .WithMessage(TranslationKey.FileExplorerSaveFailedPermission)
                .WithType(NotificationType.Error);

            notifications.Show(notification);
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            _saveSemaphore.Release();
        }

        fileTab.MarkOriginal();
        return true;
    }

    public async Task<bool> LoadAndReadFileAsync(IFileTab fileTab)
    {
        try
        {
            return await LoadFileContentAsync(fileTab);
        }
        catch (UnauthorizedAccessException)
        {
            Notification notification = Notification
                .Create(TranslationKey.FileLoadFailedTitle)
                .WithMessage(TranslationKey.FileLoadFailedPermissionMessage)
                .WithType(NotificationType.Error);

            notifications.Show(notification);
            return false;
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to load file.");
            Notification notification = Notification
                .Create(TranslationKey.FileLoadFailedTitle)
                .WithMessage(TranslationKey.FileLoadFailedGenericMessage)
                .WithType(NotificationType.Error);

            notifications.Show(notification);
            return false;
        }
    }

    private async Task<bool> LoadFileContentAsync(IFileTab fileTab)
    {
        if (string.IsNullOrEmpty(fileTab.Path))
        {
            return false;
        }

        await using Stream stream = await fileTab.File.LoadAsync();

        if (stream.IsLikelyBinary())
        {
            Dialog dialog = Dialog
                .Create(TranslationKey.BinaryFileTitle)
                .WithMessage(TranslationKey.BinaryFileFound)
                .WithIcon(Symbol.DocumentCube)
                .WithButtons(
                    new DialogButton(TranslationKey.ButtonOpen, DialogResult.Yes),
                    new DialogButton(TranslationKey.ButtonCancel, DialogResult.Cancel)
                );

            DialogResult result = await dialogs.ShowDialogAsync(dialog);

            if (result == StandardDialogResult.Cancel)
            {
                return false;
            }
        }

        using StreamReader reader = new(stream);

        string text = await reader.ReadToEndAsync();
        await fileTab.File.UpdateSnapshotAsync(stream);
        await fileTab.UpdateDocumentText(text);
        
        return true;
    }
}