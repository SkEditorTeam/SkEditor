using System;
using AvaloniaEdit;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features;

public class EditorFocusFeature : IEditorFeature
{
    private TextEditor? _editor;
    private IFocusableTab? _tab;

    public bool CanAttach(IFeatureTab tab)
    {
        return tab is IFocusableTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        if (tab is not IFocusableTab focusableTab)
        {
            return;
        }

        _tab = focusableTab;
        _editor = editor;

        _tab.FocusRequested += OnFocusRequested;
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        if (_tab != null)
        {
            _tab.FocusRequested -= OnFocusRequested;
        }

        _tab = null;
        _editor = null;
    }

    private void OnFocusRequested(object? sender, EventArgs e)
    {
        _editor?.TextArea.Focus();
    }
}