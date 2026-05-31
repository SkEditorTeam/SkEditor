using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs;
using Vector = Avalonia.Vector;

namespace SkEditor.Features;

public class ZoomFeature(AppSettings appSettings, IZoomService zoomService) : IEditorFeature
{
    private TextEditor? _editor;
    private FileTabViewModel? _tab;

    public void OnViewAttached(FileTabViewModel tab, TextEditor editor)
    {
        _tab = tab;
        _editor = editor;

        if (appSettings.IsEditorZoomLevelSyncEnabled)
        {
            _tab.FontSize = zoomService.GlobalFontSize;
        }

        _editor.FontSize = _tab.FontSize;

        zoomService.ZoomChanged += OnGlobalZoomChanged;

        _editor.AddHandler(InputElement.PointerWheelChangedEvent, OnZoom, RoutingStrategies.Tunnel);
        _editor.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
    }

    public void OnViewDetached(FileTabViewModel tab, TextEditor editor)
    {
        zoomService.ZoomChanged -= OnGlobalZoomChanged;

        _editor?.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        _editor?.RemoveHandler(InputElement.PointerWheelChangedEvent, OnZoom);

        _editor = null;
        _tab = null;
    }

    private void OnGlobalZoomChanged(double newSize)
    {
        if (!appSettings.IsEditorZoomLevelSyncEnabled || _tab == null || _editor == null)
        {
            return;
        }

        double oldLineHeight = _editor.TextArea.TextView.DefaultLineHeight;
        _tab.FontSize = newSize;

        FixScrollOffset(oldLineHeight);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyModifiers != KeyModifiers.Command)
        {
            return;
        }

        e.Handled = true;
        if (sender is not TextEditor { DataContext: FileTabViewModel })
        {
            return;
        }

        switch (e.Key)
        {
            case Key.OemPlus:
                ApplyZoom(1.0);
                break;
            case Key.OemMinus:
                ApplyZoom(-1.0);
                break;
        }
    }

    private void OnZoom(object? sender, PointerWheelEventArgs e)
    {
        if (!appSettings.IsMouseWheelZoomEnabled)
        {
            return;
        }

        if (e.KeyModifiers != KeyModifiers.Command)
        {
            return;
        }

        e.Handled = true;

        double zoomDelta = e.Delta.Y * 2.0;
        ApplyZoom(zoomDelta);
    }

    private void ApplyZoom(double delta)
    {
        if (_editor == null || _tab == null)
        {
            return;
        }

        double oldLineHeight = _editor.TextArea.TextView.DefaultLineHeight;

        if (appSettings.IsEditorZoomLevelSyncEnabled)
        {
            zoomService.GlobalFontSize += delta;
        }
        else
        {
            _tab.FontSize += delta;
            FixScrollOffset(oldLineHeight);
        }
    }

    private void FixScrollOffset(double oldLineHeight)
    {
        if (_editor == null || _tab == null)
        {
            return;
        }

        double newLineHeight = _editor.TextArea.TextView.DefaultLineHeight;
        if (!(oldLineHeight > 0) || !(newLineHeight > 0) || Equals(oldLineHeight, newLineHeight))
        {
            return;
        }

        double lineHeightChange = newLineHeight / oldLineHeight;

        double newOffset = _tab.ScrollOffset.Y * lineHeightChange;
        _tab.ScrollOffset = new Vector(_tab.ScrollOffset.X, newOffset);
    }
}