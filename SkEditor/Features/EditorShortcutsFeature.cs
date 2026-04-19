using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Input;

namespace SkEditor.Features;

public class EditorShortcutsFeature : IEditorFeature
{
    private Dictionary<(KeyGesture, KeyGesture), Action<FileTab, TextEditor>> _chordShortcuts = [];
    private TextEditor? _currentEditor;

    private List<KeyBinding> _currentKeyBindings = [];

    private FileTab? _currentTab;

    private KeyGesture? _pendingChordPrefix;
    private Dictionary<KeyGesture, Action<FileTab, TextEditor>> _shortcuts = [];

    public void Attach(FileTab fileTab)
    {
        _currentTab = fileTab;

        KeyModifiers controlModifier = KeyUtility.GetControlModifier();

        _shortcuts = new Dictionary<KeyGesture, Action<FileTab, TextEditor>>
        {
            { new KeyGesture(Key.C, controlModifier), FromCommand(ApplicationCommands.Copy) },
            { new KeyGesture(Key.V, controlModifier), FromCommand(ApplicationCommands.Paste) },
            { new KeyGesture(Key.X, controlModifier), FromCommand(ApplicationCommands.Cut) },
            { new KeyGesture(Key.Z, controlModifier), FromCommand(ApplicationCommands.Undo) },
            { new KeyGesture(Key.R, controlModifier), FromCommand(ApplicationCommands.Redo) },
            { new KeyGesture(Key.A, controlModifier), FromCommand(ApplicationCommands.SelectAll) },
            { new KeyGesture(Key.F, controlModifier), FromCommand(ApplicationCommands.Find) },
            { new KeyGesture(Key.H, controlModifier), FromCommand(ApplicationCommands.Replace) }
        };

        _chordShortcuts = new Dictionary<(KeyGesture, KeyGesture), Action<FileTab, TextEditor>>
        {
            {
                (new KeyGesture(Key.E, KeyModifiers.Control), new KeyGesture(Key.W)),
                (tab, _) => { tab.WordWrap = !tab.WordWrap; }
            }
        };

        _shortcuts.Add(new KeyGesture(Key.Back, controlModifier), (tab, editor) =>
        {
            TextDocument? doc = tab.Document;
            if (doc == null)
            {
                return;
            }

            int caretOffset = editor.CaretOffset;
            int wordStart = TextUtilities.GetNextCaretPosition(
                doc, caretOffset,
                LogicalDirection.Backward,
                CaretPositioningMode.WordStartOrSymbol);

            if (wordStart < 0)
            {
                return;
            }

            doc.Remove(wordStart, caretOffset - wordStart);
        });
    }

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        _currentEditor = editor;
        _currentKeyBindings = [];

        foreach ((KeyGesture gesture, Action<FileTab, TextEditor> action) in _shortcuts)
        {
            KeyBinding binding = new() { Gesture = gesture, Command = new RelayCommand(() => action(tab, editor)) };
            _currentKeyBindings.Add(binding);
            editor.TextArea.KeyBindings.Add(binding);
        }

        editor.TextArea.AddHandler(InputElement.KeyDownEvent, OnEditorPreviewKeyDown, RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        foreach (KeyBinding currentKeyBinding in _currentKeyBindings)
        {
            editor.TextArea.KeyBindings.Remove(currentKeyBinding);
        }

        _currentKeyBindings.Clear();
        _chordShortcuts.Clear();

        editor.TextArea.RemoveHandler(InputElement.KeyDownEvent, OnEditorPreviewKeyDown);
    }

    private static Action<FileTab, TextEditor> FromCommand(ICommand command)
    {
        return (_, _) =>
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
            return;

        var exactGesture = new KeyGesture(e.Key, e.KeyModifiers);

        if (_currentEditor == null || _currentTab == null)
        {
            return;
        }

        if (_pendingChordPrefix != null)
        {
            (KeyGesture _pendingChordPrefix, KeyGesture exactGesture) exactChord = (_pendingChordPrefix, exactGesture);

            if (_chordShortcuts.TryGetValue(exactChord, out Action<FileTab, TextEditor>? action))
            {
                action(_currentTab, _currentEditor);
            }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                var strippedGesture = new KeyGesture(e.Key, e.KeyModifiers & ~KeyModifiers.Control);
                (KeyGesture _pendingChordPrefix, KeyGesture strippedGesture) strippedChord =
                    (_pendingChordPrefix, strippedGesture);

                if (_chordShortcuts.TryGetValue(strippedChord, out action))
                {
                    action(_currentTab, _currentEditor);
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