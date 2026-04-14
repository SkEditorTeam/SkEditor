using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Input;
using AvaloniaEdit;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Input;

namespace SkEditor.Features;

public class EditorShortcutsFeature : IEditorFeature
{
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
    }

    private List<KeyBinding> _currentKeyBindings = [];
    
    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        _currentKeyBindings = [];
        
        foreach ((KeyGesture gesture, ICommand command) in _shortcuts)
        {
            KeyBinding binding = new() { Gesture = gesture, Command = command };
            _currentKeyBindings.Add(binding);
            editor.TextArea.KeyBindings.Add(binding);
        }
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        foreach (KeyBinding currentKeyBinding in _currentKeyBindings)
        {
            editor.TextArea.KeyBindings.Remove(currentKeyBinding);
        }
        
        _currentKeyBindings.Clear();
    }
}