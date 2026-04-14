using Avalonia.Input;
using AvaloniaEdit;
using SkEditor.Contracts.Services;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Input;

namespace SkEditor.Features;

public class EditorShortcutsFeature(INotificationService notifications) : IEditorFeature
{
    public bool CanAttach(FileTab tab) => true;

    public void Attach(FileTab tab)
    {
    }

    public void Detach(FileTab tab)
    {
    }

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        var controlModifier = KeyUtility.GetControlModifier();
        
        var copyBinding = new KeyBinding
        {
            Gesture = new KeyGesture(Key.C, controlModifier),
            Command = ApplicationCommands.Copy
        };
        
        var pasteBinding = new KeyBinding
        {
            Gesture = new KeyGesture(Key.V, controlModifier),
            Command = ApplicationCommands.Paste
        };

        editor.TextArea.KeyBindings.Add(copyBinding);
        editor.TextArea.KeyBindings.Add(pasteBinding);
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        notifications.Debug("view detached");
    }
}