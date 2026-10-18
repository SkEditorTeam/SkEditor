using System;
using System.IO;
using System.IO.Hashing;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit;
using FluentIcons.Common;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Models.Dialogs;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Projects;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features;

public class ChangeDetectFeature(IDialogService dialogs) : IEditorFeature
{
    private static readonly Dialog ChangesDialog = Dialog
        .Create(TranslationKey.FileChangedExternallyTitle)
        .WithMessage(TranslationKey.FileChangedExternallyMessage)
        .WithIcon(Symbol.DocumentMultipleSync)
        .WithButtons(
            new DialogButton(TranslationKey.FileChangedExternallyKeepChanges, DialogResult.Cancel, true),
            new DialogButton(TranslationKey.FileChangedExternallyDiscardAndReload, DialogResult.Ok)
        );

    private DisposableBag _disposables;
    private FileSystemWatcher? _watcher;

    public bool CanAttach(IFeatureTab tab)
    {
        return tab is ITextEditorTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        if (tab is not IFileTab fileTab || fileTab.Path is null)
        {
            return;
        }

        string path = fileTab.Path;
        string? directory = Path.GetDirectoryName(path);
        if (directory is null)
        {
            return;
        }

        _disposables = default;
        SetupFileWatcher(fileTab, editor, path, directory);
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        _disposables.Dispose();
        _watcher?.EnableRaisingEvents = false;
    }

    private void SetupFileWatcher(IFileTab fileTab, TextEditor editor, string path, string directory)
    {
        _watcher ??= new FileSystemWatcher();
        _watcher.Path = directory;
        _watcher.Filter = Path.GetFileName(path);
        _watcher.EnableRaisingEvents = true;

        _watcher
            .ChangedAsObservable().AsUnitObservable()
            .Merge(_watcher.RenamedAsObservable().AsUnitObservable())
            .Debounce(TimeSpan.FromMilliseconds(250))
            .SubscribeAwait(async (_, ct) =>
            {
                if (!File.Exists(path))
                {
                    return;
                }

                DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(path);

                if (lastWriteTimeUtc == fileTab.File.LastSaveUtc)
                {
                    return;
                }

                long fileLength = new FileInfo(path).Length;

                if (fileLength != fileTab.File.Length)
                {
                    await HandleChanged(fileTab, editor);
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
                        await HandleChanged(fileTab, editor);
                    }
                }
            })
            .AddTo(ref _disposables);
    }

    private async Task HandleChanged(IFileTab fileTab, TextEditor editor)
    {
        if (fileTab is not ISaveableTab saveable || fileTab.Path is null)
        {
            return;
        }

        if (saveable.IsSaved)
        {
            await ReloadEditorContent(fileTab, editor);
        }
        else
        {
            DialogResult result = await dialogs.ShowDialogAsync(ChangesDialog);

            if (result == StandardDialogResult.Ok)
            {
                await ReloadEditorContent(fileTab, editor);
            }
            else
            {
                await using Stream stream = await fileTab.File.LoadAsync();
                await fileTab.File.UpdateSnapshotAsync(stream);
            }
        }
    }

    private static async Task ReloadEditorContent(IFileTab fileTab, TextEditor editor)
    {
        await using Stream stream = await fileTab.File.LoadAsync();
        using StreamReader reader = new(stream);

        await fileTab.File.UpdateSnapshotAsync(stream);

        string text = await reader.ReadToEndAsync();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            editor.Document.Text = text;
            editor.Document.UndoStack.ClearAll();
            editor.Document.UndoStack.MarkAsOriginalFile();
        });
    }
}