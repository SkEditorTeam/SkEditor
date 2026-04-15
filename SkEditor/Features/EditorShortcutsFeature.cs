using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Input;

namespace SkEditor.Features;

public class EditorShortcutsFeature(INotificationService notifications) : IEditorFeature
{
    private Dictionary<(KeyGesture, KeyGesture), ICommand> _chordShortcuts = [];

    private List<KeyBinding> _currentKeyBindings = [];

    private KeyGesture? _pendingChordPrefix;
    private Dictionary<KeyGesture, ICommand> _shortcuts = [];

    public void Attach(FileTab tab)
    {
        KeyModifiers controlModifier = KeyUtility.GetControlModifier();

        _shortcuts = new Dictionary<KeyGesture, ICommand>
        {
            { new KeyGesture(Key.C, controlModifier), ApplicationCommands.Copy },
            { new KeyGesture(Key.V, controlModifier), ApplicationCommands.Paste },
            { new KeyGesture(Key.X, controlModifier), ApplicationCommands.Cut },
            { new KeyGesture(Key.Z, controlModifier), ApplicationCommands.Undo },
            { new KeyGesture(Key.R, controlModifier), ApplicationCommands.Redo },
            { new KeyGesture(Key.A, controlModifier), ApplicationCommands.SelectAll },
            { new KeyGesture(Key.F, controlModifier), ApplicationCommands.Find },
            { new KeyGesture(Key.H, controlModifier), ApplicationCommands.Replace },
        };

        _chordShortcuts = new Dictionary<(KeyGesture, KeyGesture), ICommand>
        {
            {
                (new KeyGesture(Key.E, KeyModifiers.Control), new KeyGesture(Key.W)),
                new RelayCommand(() => { notifications.Debug("toggle word wrap"); })
            }
        };
    }

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        _currentKeyBindings = [];

        foreach ((KeyGesture gesture, ICommand command) in _shortcuts)
        {
            KeyBinding binding = new() { Gesture = gesture, Command = command };
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

    private void OnEditorPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.LeftCtrl or Key.RightCtrl or Key.LeftShift or Key.RightShift or Key.LeftAlt or Key.RightAlt)
            return;

        var exactGesture = new KeyGesture(e.Key, e.KeyModifiers);

        if (_pendingChordPrefix != null)
        {
            (KeyGesture _pendingChordPrefix, KeyGesture exactGesture) exactChord = (_pendingChordPrefix, exactGesture);

            if (_chordShortcuts.TryGetValue(exactChord, out ICommand? command))
            {
                command.Execute(null);
            }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                var strippedGesture = new KeyGesture(e.Key, e.KeyModifiers & ~KeyModifiers.Control);
                (KeyGesture _pendingChordPrefix, KeyGesture strippedGesture) strippedChord =
                    (_pendingChordPrefix, strippedGesture);

                if (_chordShortcuts.TryGetValue(strippedChord, out command))
                {
                    command.Execute(null);
                }
            }

            _pendingChordPrefix = null;
            e.Handled = true;
            return;
        }

        if (_chordShortcuts.Keys.Any(k => k.Item1.Matches(e)))
        {
            _pendingChordPrefix = exactGesture;
            e.Handled = true;
        }
    }
}