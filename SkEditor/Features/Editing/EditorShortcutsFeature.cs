using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Utilities.Extensions;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features.Editing;

public class EditorShortcutsFeature : IEditorFeature
{
    private static readonly KeyModifiers ControlModifier = KeyModifiers.Command;
    private Dictionary<(KeyGesture, KeyGesture), Action> _chordShortcuts = [];

    private List<KeyBinding> _currentKeyBindings = [];

    private TextEditor? _editor;

    private KeyGesture? _pendingChordPrefix;
    private Dictionary<KeyGesture, Action> _shortcuts = [];
    private IFeatureTab? _tab;

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        _tab = tab;
        _editor = editor;
        _currentKeyBindings = [];

        _shortcuts = new Dictionary<KeyGesture, Action>
        {
            { new KeyGesture(Key.C, ControlModifier), FromCommand(ApplicationCommands.Copy) },
            { new KeyGesture(Key.V, ControlModifier), FromCommand(ApplicationCommands.Paste) },
            { new KeyGesture(Key.X, ControlModifier), FromCommand(ApplicationCommands.Cut) },
            { new KeyGesture(Key.Z, ControlModifier), FromCommand(ApplicationCommands.Undo) },
            { new KeyGesture(Key.R, ControlModifier), FromCommand(ApplicationCommands.Redo) },
            { new KeyGesture(Key.A, ControlModifier), FromCommand(ApplicationCommands.SelectAll) },
            { new KeyGesture(Key.F, ControlModifier), FromCommand(ApplicationCommands.Find) },
            { new KeyGesture(Key.H, ControlModifier), FromCommand(ApplicationCommands.Replace) },
            { new KeyGesture(Key.Back, ControlModifier), DeletePreviousWord }
        };

        _chordShortcuts = new Dictionary<(KeyGesture, KeyGesture), Action>
        {
            {
                (new KeyGesture(Key.E, KeyModifiers.Control), new KeyGesture(Key.W)),
                () => { _tab.WordWrap = !_tab.WordWrap; }
            }
        };

        foreach ((KeyGesture gesture, Action action) in _shortcuts)
        {
            KeyBinding binding = new() { Gesture = gesture, Command = new RelayCommand(action) };
            _currentKeyBindings.Add(binding);
            editor.TextArea.KeyBindings.Add(binding);
        }

        editor.TextArea.AddHandler(InputElement.KeyDownEvent, OnEditorPreviewKeyDown,
            RoutingStrategies.Tunnel, true);
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        foreach (KeyBinding binding in _currentKeyBindings)
        {
            editor.TextArea.KeyBindings.Remove(binding);
        }

        _currentKeyBindings.Clear();

        editor.TextArea.RemoveHandler(InputElement.KeyDownEvent, OnEditorPreviewKeyDown);
        _editor = null;
        _tab = null;
    }

    private void DeletePreviousWord()
    {
        if (_tab?.Document is not { } doc || _editor is not { } editor)
        {
            return;
        }

        int caretOffset = editor.CaretOffset;
        DocumentLine currentLine = doc.GetLineByOffset(caretOffset);
        int lineStartOffset = currentLine.Offset;
        int lengthToCaret = caretOffset - lineStartOffset;

        if (lengthToCaret > 0)
        {
            ReadOnlySpan<char> textToCaret =
                doc.GetTextAsMemory(lineStartOffset, lengthToCaret).Span;

            if (textToCaret.TrimStart().IsEmpty)
            {
                doc.Remove(lineStartOffset, lengthToCaret);
                return;
            }
        }

        int wordStart = TextUtilities.GetNextCaretPosition(
            doc, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStartOrSymbol);

        if (wordStart >= 0)
        {
            doc.Remove(wordStart, caretOffset - wordStart);
        }
    }

    private static Action FromCommand(ICommand command)
    {
        return () =>
        {
            if (command.CanExecute(null))
            {
                command.Execute(null);
            }
        };
    }

    private void OnEditorPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt)
        {
            return;
        }

        KeyGesture exactGesture = new(e.Key, e.KeyModifiers);

        if (_editor == null || _tab == null)
        {
            return;
        }

        if (_pendingChordPrefix != null)
        {
            (KeyGesture _pendingChordPrefix, KeyGesture exactGesture) exactChord = (_pendingChordPrefix, exactGesture);

            if (_chordShortcuts.TryGetValue(exactChord, out Action? action))
            {
                action();
            }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                KeyGesture strippedGesture = new(e.Key, e.KeyModifiers & ~KeyModifiers.Control);
                (KeyGesture _pendingChordPrefix, KeyGesture strippedGesture) strippedChord =
                    (_pendingChordPrefix, strippedGesture);

                if (_chordShortcuts.TryGetValue(strippedChord, out action))
                {
                    action();
                }
            }

            _pendingChordPrefix = null;
            e.Handled = true;
            return;
        }

        if (!_chordShortcuts.Keys.Any(k => k.Item1.Matches(e)))
        {
            return;
        }

        _pendingChordPrefix = exactGesture;
        e.Handled = true;
    }
}