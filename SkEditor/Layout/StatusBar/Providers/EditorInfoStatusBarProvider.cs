using System;
using System.Collections.Generic;
using System.ComponentModel;
using AvaloniaEdit.Document;
using R3;
using SkEditor.Core.Localization;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Layout.StatusBar.UI;
using SkEditor.Windowing;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Layout.StatusBar.Providers;

public class EditorInfoStatusBarProvider : IStatusBarProvider, IDisposable
{
    private readonly StatusBarItem _cursorItem;
    private readonly ITabService _tabs;
    private DisposableBag _disposables;

    public ReadOnlyReactiveProperty<bool> CanAttach { get; }

    public EditorInfoStatusBarProvider(ITabService tabs, IWindowDialogService windowDialogs)
    {
        _tabs = tabs;

        _cursorItem = new StatusBarItem()
            .WithAlignment(StatusBarItem.ItemAlignment.Right)
            .WithCommand(windowDialogs.ShowGoToLineDialogAsync);

        CanAttach = Observable.FromEvent<ITab?>(
                h => _tabs.SelectedTabChanged += h,
                h => _tabs.SelectedTabChanged -= h
            )
            .Select(tab => tab is ITextEditorTab)
            .Prepend(_tabs.SelectedTab is ITextEditorTab)
            .ToReadOnlyReactiveProperty();

        Observable.FromEvent<ITab?>(
                h => _tabs.SelectedTabChanged += h,
                h => _tabs.SelectedTabChanged -= h
            )
            .Prepend(_tabs.SelectedTab)
            .Select(tab =>
            {
                if (tab is ITextEditorTab editorTab and INotifyPropertyChanged npc)
                {
                    return Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                            h => (_, e) => h(e),
                            h => npc.PropertyChanged += h,
                            h => npc.PropertyChanged -= h
                        )
                        .Where(e => e.PropertyName == nameof(FileTabViewModel.CaretOffset))
                        .Select(_ => editorTab)
                        .Prepend(editorTab);
                }

                return Observable.Return<ITextEditorTab?>(null)!;
            })
            .Switch()
            .Subscribe(UpdateState)
            .AddTo(ref _disposables);
    }

    public IEnumerable<StatusBarItem> GetItems()
    {
        yield return _cursorItem;
    }

    private void UpdateState(ITextEditorTab? editorTab)
    {
        if (editorTab is not null)
        {
            _cursorItem.IsVisible = true;

            int offset = editorTab.CaretOffset;
            TextLocation location = editorTab.Document.GetLocation(offset);

            _cursorItem.Text = TranslatableString.FromRawString($"{location.Line}:{location.Column}");
        }
        else
        {
            _cursorItem.IsVisible = false;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _disposables.Dispose();
    }
}