using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using SkEditor.Contracts.Services;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Input;
using SkEditor.Utilities.Settings.Configs;
using Vector = Avalonia.Vector;

namespace SkEditor.Features;

public class ZoomFeature(AppSettings appSettings) : IEditorFeature
{
    private readonly Dictionary<FileTab, TextEditor> _activeEditors = [];
    private readonly HashSet<FileTab> _allAttachedTabs = [];
    
    private double? _syncedFontSize;

    public bool CanAttach(FileTab tab) => true;

    public void Attach(FileTab tab)
    {
        _allAttachedTabs.Add(tab);
        
        if (appSettings.IsEditorZoomLevelSyncEnabled && _syncedFontSize.HasValue)
        {
            tab.FontSize = _syncedFontSize.Value;
        }
    }

    public void Detach(FileTab tab)
    {
        _allAttachedTabs.Remove(tab);
    }

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        _activeEditors[tab] = editor;
        editor.AddHandler(InputElement.PointerWheelChangedEvent, OnZoom, RoutingStrategies.Tunnel);
        editor.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        _activeEditors.Remove(tab);
        editor.RemoveHandler(InputElement.PointerWheelChangedEvent, OnZoom);
        editor.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyUtility.GetControlModifier()) return;

        e.Handled = true;
        if (sender is not TextEditor { DataContext: FileTab } editor) return;

        switch (e.Key)
        {
            case Key.OemPlus:
                ZoomEditors(1.0, editor);
                break;
            case Key.OemMinus:
                ZoomEditors(-1.0, editor);
                break;
        }
    }

    private void OnZoom(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers != KeyUtility.GetControlModifier()) return;

        e.Handled = true;
        if (sender is not TextEditor editor) return;

        double zoomDelta = e.Delta.Y * 2.0;

        ZoomEditors(zoomDelta, editor);
    }

    private void ZoomEditors(double zoomDelta, TextEditor editor)
    {
        if (appSettings.IsEditorZoomLevelSyncEnabled)
        {
            foreach (var tab in _allAttachedTabs)
            {
                if (_activeEditors.TryGetValue(tab, out var activeEditor))
                {
                    ApplyZoom(tab, activeEditor, zoomDelta);
                }
                else
                {
                    tab.FontSize += zoomDelta;
                    if (tab.FontSize < 5) tab.FontSize = 5;
                }
            }
            
            _syncedFontSize = _allAttachedTabs.FirstOrDefault()?.FontSize;
        }
        else if (editor.DataContext is FileTab tab)
        {
            ApplyZoom(tab, editor, zoomDelta);
        }
    }

    private static void ApplyZoom(FileTab tab, TextEditor editor, double delta)
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

        var scrollViewer = editor.ScrollViewer;
        if (scrollViewer == null) return;

        double newOffset = scrollViewer.Offset.Y * lineHeightChange;
        scrollViewer.Offset = new Vector(scrollViewer.Offset.X, newOffset);
    }
}