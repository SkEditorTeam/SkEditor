using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit;
using R3;
using SkEditor.Editor.Tabs.Capabilities;

namespace SkEditor.Editor.Features.Editing;

public class AltKeyHandlingFeature : IEditorFeature
{
    private CompositeDisposable? _disposables;

    public bool CanAttach(IFeatureTab tab)
    {
        return tab is ITextEditorTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        if (tab is not ITextEditorTab)
        {
            return;
        }

        _disposables = [];

        Observable.FromEventHandler<VisualTreeAttachmentEventArgs>(
                h => editor.AttachedToVisualTree += h,
                h => editor.AttachedToVisualTree -= h)
            .Select(_ => TopLevel.GetTopLevel(editor))
            .Where(topLevel => topLevel != null)
            .Subscribe(topLevel => RegisterTopLevelHandler(editor, topLevel!, _disposables))
            .AddTo(_disposables);

        if (!editor.IsAttachedToVisualTree())
        {
            return;
        }

        TopLevel? topLevel = TopLevel.GetTopLevel(editor);
        if (topLevel != null)
        {
            RegisterTopLevelHandler(editor, topLevel, _disposables);
        }
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        _disposables?.Dispose();
        _disposables = null;
    }

    private static void RegisterTopLevelHandler(TextEditor editor, TopLevel topLevel, CompositeDisposable disposables)
    {
        EventHandler<KeyEventArgs> keyDownHandler = (_, e) => HandleKeyDown(editor, topLevel, e);

        topLevel.AddHandler(InputElement.KeyDownEvent, keyDownHandler, RoutingStrategies.Tunnel);

        Disposable.Create(() => { topLevel.RemoveHandler(InputElement.KeyDownEvent, keyDownHandler); })
            .AddTo(disposables);
    }

    private static void HandleKeyDown(TextEditor editor, TopLevel topLevel, KeyEventArgs e)
    {
        if (!editor.TextArea.IsFocused)
        {
            return;
        }

        if (e.Key is not (Key.LeftAlt or Key.RightAlt))
        {
            return;
        }

        bool hasOtherModifiers = e.KeyModifiers.HasFlag(KeyModifiers.Shift)
                                 || e.KeyModifiers.HasFlag(KeyModifiers.Control)
                                 || e.KeyModifiers.HasFlag(KeyModifiers.Meta);

        if (!hasOtherModifiers)
        {
            return;
        }

        // I really don't like this hacky approach but that's the only way I've found.
        // Avalonia's IAccessKeyHandler is internal so I can't even provide my own implementation
        Dispatcher.UIThread.Post(() =>
        {
            topLevel.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Key = Key.None
            });
        });
    }
}