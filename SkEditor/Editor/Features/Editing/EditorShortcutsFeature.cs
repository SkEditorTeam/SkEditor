using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Layout.Menu;

namespace SkEditor.Editor.Features.Editing;

public class EditorShortcutsFeature : IEditorFeature
{
    private readonly KeyModifiers _ctrlMod = KeyGestureUtilities.PlatformControlModifier;
    
    private Dictionary<(KeyGesture, KeyGesture), Action> _chordShortcuts = [];

    private List<KeyBinding> _currentKeyBindings = [];

    private TextEditor? _editor;

    private KeyGesture? _pendingChordPrefix;
    private Dictionary<KeyGesture, Action> _shortcuts = [];
    private ITextEditorTab? _tab;

    public bool CanAttach(IFeatureTab tab)
    {
        return tab is ITextEditorTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        if (tab is not ITextEditorTab textEditorTab)
        {
            return;
        }

        _tab = textEditorTab;
        _editor = editor;
        _currentKeyBindings = [];

        _shortcuts = new Dictionary<KeyGesture, Action>
        {
            { new KeyGesture(Key.C, _ctrlMod), FromCommand(ApplicationCommands.Copy) },
            { new KeyGesture(Key.V, _ctrlMod), FromCommand(ApplicationCommands.Paste) },
            { new KeyGesture(Key.X, _ctrlMod), FromCommand(ApplicationCommands.Cut) },
            { new KeyGesture(Key.Z, _ctrlMod), FromCommand(ApplicationCommands.Undo) },
            { new KeyGesture(Key.R, _ctrlMod), FromCommand(ApplicationCommands.Redo) },
            { new KeyGesture(Key.A, _ctrlMod), FromCommand(ApplicationCommands.SelectAll) },
            { new KeyGesture(Key.F, _ctrlMod), FromCommand(ApplicationCommands.Find) },
            { new KeyGesture(Key.H, _ctrlMod), FromCommand(ApplicationCommands.Replace) },
            { new KeyGesture(Key.Back, _ctrlMod), () => TextEditorUtilities.DeletePreviousWord(editor) }
        };

        _chordShortcuts = new Dictionary<(KeyGesture, KeyGesture), Action>
        {
            {
                (new KeyGesture(Key.E, _ctrlMod), new KeyGesture(Key.W)),
                () => { _tab.WordWrap.Value = !_tab.WordWrap.Value; }
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
        if (_editor == null || _tab == null)
        {
            return;
        }
        
        if (CheckLineMoveShortcuts(e))
        {
            return;
        }

        if (CheckSelectionCaretMovement(e))
        {
            return;
        }

        if (e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt)
        {
            return;
        }

        KeyGesture exactGesture = new(e.Key, e.KeyModifiers);

        if (_pendingChordPrefix != null)
        {
            (KeyGesture _pendingChordPrefix, KeyGesture exactGesture) exactChord = (_pendingChordPrefix, exactGesture);
            bool matched = false;

            if (_chordShortcuts.TryGetValue(exactChord, out Action? action))
            {
                action();
                matched = true;
            }
            else if (e.KeyModifiers.HasFlag(_ctrlMod))
            {
                KeyGesture strippedGesture = new(e.Key, e.KeyModifiers & ~_ctrlMod);
                (KeyGesture _pendingChordPrefix, KeyGesture strippedGesture) strippedChord =
                    (_pendingChordPrefix, strippedGesture);

                if (_chordShortcuts.TryGetValue(strippedChord, out action))
                {
                    action();
                    matched = true;
                }
            }

            _pendingChordPrefix = null;
            if (matched || e.Key == Key.Escape)
            {
                e.Handled = true;
            }
            return;
        }

        if (!_chordShortcuts.Keys.Any(k => k.Item1.Matches(e)))
        {
            return;
        }

        _pendingChordPrefix = exactGesture;
        e.Handled = true;
    }

    private bool CheckSelectionCaretMovement(KeyEventArgs e)
    {
        if (_editor is null)
        {
            return false;
        }

        if (e.KeyModifiers != KeyModifiers.None || _editor.SelectionLength <= 0)
        {
            return false;
        }

        switch (e.Key)
        {
            case Key.Left:
                _editor.CaretOffset = _editor.SelectionStart;
                _editor.TextArea.ClearSelection();
                e.Handled = true;
                return true;

            case Key.Right:
                _editor.CaretOffset = _editor.SelectionStart + _editor.SelectionLength;
                _editor.TextArea.ClearSelection();
                e.Handled = true;
                return true;
        }

        return false;
    }

    private bool CheckLineMoveShortcuts(KeyEventArgs e)
    {
        if (_editor is null)
        {
            return false;
        }

        if (e.KeyModifiers != KeyModifiers.Alt || e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            return false;
        }

        switch (e.Key)
        {
            case Key.Up:
                TextEditorUtilities.MoveSelectionUp(_editor);
                e.Handled = true;
                return true;
            case Key.Down:
                TextEditorUtilities.MoveSelectionDown(_editor);
                e.Handled = true;
                return true;
        }

        return false;
    }
}