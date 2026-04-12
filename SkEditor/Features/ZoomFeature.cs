using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using AvaloniaEdit;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Input;
using SkEditor.Utilities.Settings.Configs;
using Vector = Avalonia.Vector;

namespace SkEditor.Features;

public class ZoomFeature(AppSettings appSettings) : IEditorFeature
{
    private readonly Dictionary<FileTab, TextEditor> _activeEditors = [];

    public bool CanAttach(FileTab tab) => true;

    public void Attach(FileTab tab)
    {
    }

    public void Detach(FileTab tab)
    {
    }

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        _activeEditors[tab] = editor;
        editor.AddHandler(InputElement.PointerWheelChangedEvent, OnZoom, RoutingStrategies.Tunnel);
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        _activeEditors.Remove(tab);
        editor.RemoveHandler(InputElement.PointerWheelChangedEvent, OnZoom);
    }

    private void OnZoom(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers != KeyUtility.GetControlModifier()) return;

        e.Handled = true;
        if (sender is not TextEditor editor) return;

        double zoomDelta = e.Delta.Y * 2.0;

        if (appSettings.IsEditorZoomLevelSyncEnabled)
        {
            foreach (var entry in _activeEditors)
            {
                ApplyZoom(entry.Key, entry.Value, zoomDelta);
            }
        }
        else if (editor.DataContext is FileTab tab)
        {
            ApplyZoom(tab, editor, zoomDelta);
        }
    }

    private void ApplyZoom(FileTab tab, TextEditor editor, double delta)
    {
        switch (delta)
        {
            case < 0 when tab.FontSize <= 5:
            case > 0 when tab.FontSize >= 200:
                return;
        }

        double oldLineHeight = editor.TextArea.TextView.DefaultLineHeight;

        tab.FontSize += delta;
        if (tab.FontSize < 5) tab.FontSize = 5;

        double newLineHeight = editor.TextArea.TextView.DefaultLineHeight;
        if (oldLineHeight <= 0 || newLineHeight <= 0) return;

        double lineHeightChange = newLineHeight / oldLineHeight;

        var scrollViewer = editor.GetVisualChildren().OfType<ScrollViewer>().FirstOrDefault();
        if (scrollViewer == null)
        {
            return;
        }

        double newOffset = scrollViewer.Offset.Y * lineHeightChange;
        scrollViewer.Offset = new Vector(scrollViewer.Offset.X, newOffset);
    }
}