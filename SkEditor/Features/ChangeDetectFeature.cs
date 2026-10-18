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

                if (lastWriteTimeUtc != fileTab.File.LastSaveUtc)
                {
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
            await using Stream stream = await fileTab.File.LoadAsync();
            using StreamReader reader = new(stream);

            string text = await reader.ReadToEndAsync();
            await fileTab.File.UpdateSnapshotAsync(stream);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                editor.Document.Text = text;
                editor.Document.UndoStack.ClearAll();
                editor.Document.UndoStack.MarkAsOriginalFile();
            });
        }
        else
        {
            Dialog dialog = Dialog
                .Create(TranslationKey.FileChangedExternallyTitle)
                .WithMessage(TranslationKey.FileChangedExternallyMessage)
                .WithIcon(Symbol.DocumentMultipleSync)
                .WithButtons(
                    new DialogButton(TranslationKey.FileChangedExternallyKeepChanges, DialogResult.Cancel, true),
                    new DialogButton(TranslationKey.FileChangedExternallyDiscardAndReload, DialogResult.Ok)
                );

            DialogResult result = await dialogs.ShowDialogAsync(dialog);

            if (result == StandardDialogResult.Ok)
            {
                await using Stream stream = await fileTab.File.LoadAsync();
                using StreamReader reader = new(stream);

                string text = await reader.ReadToEndAsync();
                await fileTab.File.UpdateSnapshotAsync(stream);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    editor.Document.Text = text;
                    editor.Document.UndoStack.ClearAll();
                    editor.Document.UndoStack.MarkAsOriginalFile();
                });
            }
            else
            {
                await using Stream stream = await fileTab.File.LoadAsync();
                using StreamReader reader = new(stream);
                await fileTab.File.UpdateSnapshotAsync(stream);
            }
        }
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        _disposables.Dispose();
        _watcher?.EnableRaisingEvents = false;
    }
}