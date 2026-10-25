using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using FluentIcons.Common;
using MemoryPack;
using Microsoft.Win32.SafeHandles;
using SkEditor.Core.Common;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Windowing.Errors.Models;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Windowing.Errors;

public class CrashService(
    IFileSystem fileSystem,
    ITabService tabs,
    ILogService logs,
    IFileTabFactory fileTabFactory,
    IFileService files,
    IDialogService dialogs) : ICrashService
{
    public static readonly string CrashLogFilePath = Path.Combine(AppPaths.LocalData, "crash.log");
    public static readonly string CrashRecoveryFilePath = Path.Combine(AppPaths.LocalData, "crash_recovery.bin");

    public void SaveCrashState(Exception ex)
    {
        SaveCrashLog(ex);

        CrashStateFile[] stateFiles = tabs.AllTabs
            .OfType<IFileTab>()
            .Select(tab =>
            {
                bool isSaved = tab is { IsSaved: true };
                string? content = null;
                if (tab is ITextEditorTab editorTab)
                {
                    if (!isSaved || tab.File is SftpFile)
                    {
                        content = editorTab.Document.Text;
                    }
                }

                return new CrashStateFile
                {
                    IsSaved = isSaved,
                    Path = tab.Path,
                    Header = tab.Header.Value,
                    Content = content,
                    IsRemote = tab.File is SftpFile
                };
            })
            .ToArray();

        CrashState state = new(stateFiles);

        fileSystem.Directory.CreateDirectory(AppPaths.LocalData);

        ArrayBufferWriter<byte> writer = new();
        MemoryPackSerializer.Serialize(writer, state);
        using SafeFileHandle handle = fileSystem.File.OpenHandle(
            CrashRecoveryFilePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None);

        fileSystem.RandomAccess.Write(handle, writer.WrittenSpan, fileOffset: 0);
    }

    private void SaveCrashLog(Exception ex)
    {
        try
        {
            string dir = Path.GetDirectoryName(CrashLogFilePath)!;
            if (!fileSystem.Directory.Exists(dir))
            {
                fileSystem.Directory.CreateDirectory(dir);
            }

            string logContent = $"""
                                 --------
                                 SkEditor Crash Report - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
                                 OS: {Environment.OSVersion} ({System.Runtime.InteropServices.RuntimeInformation.OSDescription})
                                 Arch: {System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture}
                                 CommandLine: {Environment.CommandLine}
                                 --------

                                 Exception:
                                 {ex}
                                 """;

            fileSystem.File.WriteAllText(CrashLogFilePath, logContent);
        }
        catch (Exception logEx)
        {
            logs.Error(logEx, "Failed to write crash.log");
        }
    }

    public async Task CheckForCrashes()
    {
        if (!fileSystem.File.Exists(CrashRecoveryFilePath))
        {
            return;
        }

        // ReSharper disable once MethodHasAsyncOverload
        byte[] bytes = fileSystem.File.ReadAllBytes(CrashRecoveryFilePath);
        CrashState? state;
        try
        {
            state = MemoryPackSerializer.Deserialize<CrashState>(bytes);
        }
        catch (Exception e)
        {
            fileSystem.File.Delete(CrashRecoveryFilePath);
            logs.Error(e, "Restoring pre-crash state failed");
            return;
        }

        if (state is null || state.Files.Length == 0)
        {
            fileSystem.File.Delete(CrashRecoveryFilePath);
            return;
        }
        
        int unsavedFiles = state.Files.Count(f => !f.IsSaved || f.IsRemote);

        TranslationKey message = unsavedFiles > 0
            ? TranslationKey.CrashRecoveryMessageUnsaved
            : TranslationKey.CrashRecoveryMessageAllSaved;

        Dialog dialog = Dialog
            .Create(TranslationKey.CrashRecoveryTitle)
            .WithMessage(message)
            .WithIcon(Symbol.ShieldError)
            .WithButtons(
                new DialogButton(TranslationKey.CrashRecoveryRestore, DialogResult.Yes, isDefault: true),
                new DialogButton(TranslationKey.CrashRecoveryDiscard, DialogResult.No)
            );

        DialogResult result = await dialogs.ShowDialogAsync(dialog);

        if (result != DialogResult.Yes)
        {
            fileSystem.File.Delete(CrashRecoveryFilePath);
            return;
        }

        try
        {
            List<Task> restoreTasks = [];
            foreach (CrashStateFile file in state.Files)
            {
                FileTabViewModel? tab;
                if (file.Content is null && file.Path is not null && file.IsSaved)
                {
                    if (!fileSystem.File.Exists(file.Path))
                    {
                        continue;
                    }

                    tab = fileTabFactory.CreateFromFile(new LocalFile(file.Path));
                    tabs.OpenTab(tab);
                    restoreTasks.Add(files.LoadAndReadFileAsync(tab));
                }
                else
                {
                    tab = fileTabFactory.CreateFromCrash(file);
                    tab.Document.Text = file.Content ?? string.Empty;
                    tab.Document.UndoStack.ClearAll();
                    tabs.OpenTab(tab);
                }
            }

            await Task.WhenAll(restoreTasks);
        }
        finally
        {
            if (fileSystem.File.Exists(CrashRecoveryFilePath))
            {
                fileSystem.File.Delete(CrashRecoveryFilePath);
            }
        }
    }
}