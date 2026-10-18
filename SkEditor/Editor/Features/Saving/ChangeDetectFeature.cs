using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Hashing;
using System.Threading;
using System.Threading.Tasks;
using FluentIcons.Common;
using R3;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Tabs.Capabilities;
using AppSettings = SkEditor.Configuration.Configs.AppSettings;

namespace SkEditor.Editor.Features.Saving;

public class ChangeDetectFeature(AppSettings appSettings, IDialogService dialogs, IFileSystem? fileSystem = null)
    : IEditorFeature
{
    private static readonly Dialog ChangesDialog = Dialog
        .Create(TranslationKey.FileChangedExternallyTitle)
        .WithMessage(TranslationKey.FileChangedExternallyMessage)
        .WithIcon(Symbol.DocumentSync)
        .WithButtons(
            new DialogButton(TranslationKey.FileChangedExternallyKeepChanges, DialogResult.Cancel, true),
            new DialogButton(TranslationKey.FileChangedExternallyDiscardAndReload, DialogResult.Ok)
        );

    private static readonly IFileSystem DefaultFileSystem = new FileSystem();
    private readonly IFileSystem _fileSystem = fileSystem ?? DefaultFileSystem;

    private DisposableBag _disposables;
    private FileSystemWatcher? _watcher;

    public bool CanAttach(IFeatureTab tab)
    {
        return appSettings.IsChangeDetectEnabled && tab is ITextEditorTab and IFileTab { File: LocalFile };
    }

    public void Attach(IFeatureTab tab)
    {
        if (tab is not IFileTab fileTab || fileTab.Path is null)
        {
            return;
        }

        string path = fileTab.Path;
        string? directory = _fileSystem.Path.GetDirectoryName(path);
        if (directory is null)
        {
            return;
        }

        _disposables.Dispose();
        _disposables = default;
        SetupFileWatcher(fileTab, path, directory);
    }

    public void Detach(IFeatureTab tab)
    {
        _disposables.Dispose();
        _watcher?.EnableRaisingEvents = false;
    }

    public async Task CheckChangesAsync(IFileTab fileTab, CancellationToken ct = default)
    {
        string? path = fileTab.Path;
        if (!_fileSystem.File.Exists(path) || path is null)
        {
            return;
        }

        DateTime lastWriteTimeUtc = _fileSystem.File.GetLastWriteTimeUtc(path);

        if (lastWriteTimeUtc == fileTab.File.LastSaveUtc)
        {
            return;
        }

        long fileLength = _fileSystem.FileInfo.New(path).Length;

        if (fileLength != fileTab.File.Length)
        {
            await HandleChanged(fileTab);
        }
        else
        {
            ulong? originalHash = fileTab.File.DiskHash;
            XxHash3 hasher = new();
            await using Stream stream = await fileTab.File.LoadAsync();
            await hasher.AppendAsync(stream, ct);
            ulong? diskHash = hasher.GetCurrentHashAsUInt64();

            if (originalHash != diskHash)
            {
                await HandleChanged(fileTab);
            }
            else
            {
                fileTab.File.LastSaveUtc = lastWriteTimeUtc;
            }
        }
    }

    private void SetupFileWatcher(IFileTab fileTab, string path, string directory)
    {
        _watcher ??= new FileSystemWatcher();
        _watcher.Path = directory;
        _watcher.Filter = _fileSystem.Path.GetFileName(path);
        _watcher.EnableRaisingEvents = true;

        _watcher
            .ChangedAsObservable().AsUnitObservable()
            .Merge(_watcher.RenamedAsObservable().AsUnitObservable())
            .Debounce(TimeSpan.FromMilliseconds(250))
            .ObserveOnUIThreadDispatcher()
            .SubscribeAwait(async (_, ct) => { await CheckChangesAsync(fileTab, ct); })
            .AddTo(ref _disposables);
    }

    private async Task HandleChanged(IFileTab fileTab)
    {
        if (fileTab is not ISaveableTab saveable || fileTab.Path is null)
        {
            return;
        }

        if (saveable.IsSaved)
        {
            await ReloadEditorContent(fileTab);
        }
        else
        {
            DialogResult result = await dialogs.ShowDialogAsync(ChangesDialog);

            if (result == StandardDialogResult.Ok)
            {
                await ReloadEditorContent(fileTab);
            }
            else
            {
                await using Stream stream = await fileTab.File.LoadAsync();
                await fileTab.File.UpdateSnapshotAsync(stream);
            }
        }
    }

    private static async Task ReloadEditorContent(IFileTab fileTab)
    {
        await using Stream stream = await fileTab.File.LoadAsync();
        using StreamReader reader = new(stream);

        await fileTab.File.UpdateSnapshotAsync(stream);

        string text = await reader.ReadToEndAsync();

        if (fileTab is not ITextEditorTab editorTab)
        {
            return;
        }

        int caretOffset = editorTab.CaretOffset;
        editorTab.Document.Text = text;
        editorTab.Document.UndoStack.ClearAll();
        editorTab.Document.UndoStack.MarkAsOriginalFile();
        editorTab.CaretOffset = Math.Min(caretOffset, text.Length);
    }
}