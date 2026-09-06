using System.Collections.Generic;
using System.IO;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using R3;
using SkEditor.Utilities.Editor.Completions;
using SkEditor.Utilities.Extensions;
using SkEditor.ViewModels.Tabs.Capabilities;
using SkEditor.Views.Controls.Editor;

namespace SkEditor.Features;

public class CompletionFeature(IEnumerable<ICompletionProvider> completionProviders) : IEditorFeature
{
    private SkEditorCompletionWindow? _completionWindow;
    private TextEditor? _editor;

    private DisposableBag _disposables;

    public bool CanAttach(IFeatureTab tab)
    {
        if (tab is not IFileTab fileTab)
        {
            return false;
        }

        string? extension = Path.GetExtension(fileTab.Path ?? null)?.ToLowerInvariant();

        return string.IsNullOrEmpty(extension) || extension == ".sk";
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        _disposables = default;
        _editor = editor;

        editor.TextEnteredAsObservable()
            .Where(static e => !string.IsNullOrEmpty(e.Text) && ShouldTriggerCompletion(e.Text))
            .Subscribe(_ => OpenCompletionWindow(editor))
            .AddTo(ref _disposables);

        editor.ObservePropertyChanged(x => x.FontSize)
            .Subscribe(_ => _completionWindow?.Close())
            .AddTo(ref _disposables);

        editor.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);

        _disposables.Add(Disposable.Create(() => { editor.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown); }));
    }


    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        _disposables.Dispose();
        _editor = null;
        _completionWindow?.Close();
        _completionWindow = null;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Space || e.KeyModifiers != KeyModifiers.Control)
        {
            return;
        }

        if (_editor is null)
        {
            return;
        }

        e.Handled = true;

        int offset = _editor.CaretOffset;

        if (offset <= 0)
        {
            return;
        }

        char previous = _editor.Document.GetCharAt(offset - 1);
        if (!ShouldTriggerCompletion(previous))
        {
            return;
        }

        OpenCompletionWindow(_editor);
    }

    private void OpenCompletionWindow(TextEditor editor)
    {
        if (_completionWindow?.IsOpen == true)
        {
            return;
        }

        _completionWindow = new SkEditorCompletionWindow(editor.TextArea, completionProviders);
        _completionWindow.Closed += (_, _) => _completionWindow = null;
        _completionWindow.Show();
    }

    private static bool ShouldTriggerCompletion(string text)
    {
        char c = text[0];
        return ShouldTriggerCompletion(c);
    }

    private static bool ShouldTriggerCompletion(char c)
    {
        return char.IsLetterOrDigit(c)
               || c == '_'
               || c == '{'
               || c == '@'
               || c == ':'
               || c == '%'
               || c == '/';
    }
}