using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using SkEditor.API;

namespace SkEditor.Utilities.Files;

public static class SessionRestorer
{
    public const string SessionFileExtension = ".skeditor";
    public const string LockFileName = "session.lock";

    public static readonly string SessionFolder = Path.Combine(Path.GetTempPath(), "SkEditor", "Session");
    public static readonly string LockFilePath = Path.Combine(SessionFolder, LockFileName);

    private static readonly SemaphoreSlim SessionSemaphore = new(1, 1);

    public static async Task<bool> SaveSession(CancellationToken cancellationToken = default)
    {
        try
        {
            await SessionSemaphore.WaitAsync(cancellationToken);

            try
            {
                SessionFileHandler.EnsureSessionDirectoryExists();

                await SessionFileHandler.CreateLockFile();

                List<OpenedFile> openedFiles = SkEditorAPI.Files.GetOpenedFiles();
                int savedCount = 0;

                await SessionFileHandler.SafelyClearSessionFolder();

                try
                {
                    await SessionProjectHandler.SaveProjectFolder();
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to save project folder information");
                }

                int index = 0;
                foreach (OpenedFile openedFile in openedFiles)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Log.Warning("Session save was cancelled");
                        return false;
                    }

                    if (!openedFile.IsEditor || string.IsNullOrEmpty(openedFile.Editor?.Text))
                    {
                        continue;
                    }

                    try
                    {
                        string jsonData = SessionSerializer.BuildSavingData(openedFile);

                        if (Encoding.UTF8.GetByteCount(jsonData) > SessionCompresser.MaxCompressionSize)
                        {
                            Log.Warning("File too large to save in session: {Path}", openedFile.Path ?? "Untitled");
                            continue;
                        }

                        string compressed = await SessionCompresser.Compress(jsonData);
                        string path = Path.Combine(SessionFolder, $"file_{index}{SessionFileExtension}");
                        await SessionFileHandler.WriteFile(path, compressed);
                        savedCount++;
                        index++;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        Log.Error(ex, "Failed to save session file {Index}", index);
                    }
                }

                Log.Information("Session saved successfully. Saved {Count} files.", savedCount);
                return savedCount > 0;
            }
            finally
            {
                try
                {
                    if (File.Exists(LockFilePath))
                    {
                        File.Delete(LockFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to delete lock file");
                }

                SessionSemaphore.Release();
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("Session save operation was cancelled");
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save session");
            return false;
        }
    }

    public static async Task<bool> RestoreSession(CancellationToken cancellationToken = default)
    {
        try
        {
            await SessionSemaphore.WaitAsync(cancellationToken);

            try
            {
                if (!Directory.Exists(SessionFolder))
                {
                    return false;
                }

                if (File.Exists(LockFilePath))
                {
                    FileInfo lockFile = new(LockFilePath);
                    if ((DateTime.Now - lockFile.LastWriteTime).TotalMinutes < 5)
                    {
                        Log.Warning("Session is locked. Another instance might be saving.");
                        return false;
                    }

                    try
                    {
                        File.Delete(LockFilePath);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to delete stale lock file");
                        return false;
                    }
                }

                string[] files = SessionFileHandler.GetSessionFiles();
                if (files.Length == 0)
                {
                    return false;
                }

                int restoredCount = 0;
                int failedCount = 0;

                await SessionFileHandler.CreateLockFile();

                try
                {
                    bool projectRestored = await SessionProjectHandler.RestoreProjectFolder();
                    if (projectRestored)
                    {
                        Log.Information("Project folder restored successfully");
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to restore project folder");
                }

                foreach (string file in files)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Log.Warning("Session restore was cancelled");
                        return restoredCount > 0;
                    }

                    try
                    {
                        string compressed = await SessionFileHandler.ReadFile(file);
                        if (string.IsNullOrEmpty(compressed))
                        {
                            Log.Warning("Empty session file: {File}", file);
                            continue;
                        }

                        string jsonData = await SessionCompresser.Decompress(compressed);
                        if (string.IsNullOrEmpty(jsonData))
                        {
                            Log.Warning("Failed to decompress session file: {File}", file);
                            continue;
                        }

                        SessionSerializer.SessionFileData fileData = SessionSerializer.BuildOpeningData(jsonData);
                        if (fileData.Content == null)
                        {
                            Log.Warning("Invalid session data in file: {File}", file);
                            continue;
                        }

                        if (!string.IsNullOrEmpty(fileData.Path) && !SessionFileHandler.IsPathValid(fileData.Path))
                        {
                            Log.Warning("Invalid file path in session: {Path}", fileData.Path);
                            fileData.Path = null;
                        }

                        SkEditorAPI.Logs.Info(
                            $"Path: {fileData.Path}, hasUnsavedChanges: {fileData.HasUnsavedChanges}");

                        OpenedFile openedFile = await SkEditorAPI.Files.AddEditorTab(fileData.Content, fileData.Path);
                        openedFile.IsSaved = !fileData.HasUnsavedChanges;

                        restoredCount++;
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        Log.Error(ex, "Failed to restore session file: {File}", file);
                        failedCount++;
                    }
                }

                Log.Information("Session restored. Opened {Success} files. Failed to open {Failed} files.",
                    restoredCount, failedCount);

                return restoredCount > 0;
            }
            finally
            {
                try
                {
                    if (File.Exists(LockFilePath))
                    {
                        File.Delete(LockFilePath);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to delete lock file");
                }

                SessionSemaphore.Release();
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("Session restore operation was cancelled");
            return false;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to restore session");
            return false;
        }
    }

    public static async Task ClearSession()
    {
        try
        {
            await SessionSemaphore.WaitAsync();

            try
            {
                await SessionFileHandler.SafelyClearSessionFolder();
                Log.Information("Session cleared successfully");
            }
            finally
            {
                SessionSemaphore.Release();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to clear session");
        }
    }
}