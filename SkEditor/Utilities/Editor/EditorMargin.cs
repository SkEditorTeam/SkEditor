using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using SkEditor.API;
using SkEditor.Utilities.Files;

namespace SkEditor.Utilities.Editor;

/// <summary>
/// Represent the left margin column of the code editor. This margin is used to display
/// icons on the left of the editor, next to the line numbers.
///
/// Can be accessed from any <see cref="OpenedFile"/> as <code>Margin</code> custom data key (<see cref="OpenedFile.CustomData"/>)
/// </summary>
public class EditorMargin : AbstractMargin
{
    public Dictionary<int, MarginIconData> RenderedIcons { get; } = new();
    public MarginIconData? HoveredIcon { get; set; }
    
    public OpenedFile File { get; }
    public EditorMargin(OpenedFile file)
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
        File = file;
        
        file.Editor.TextArea.LeftMargins.Insert(0, this);
        file.Editor.TextChanged += (_, _) => Reload();
        Reload();
    }

    protected override void OnTextViewChanged(TextView? oldTextView, TextView? newTextView)
    {
        if (oldTextView != null)
            oldTextView.VisualLinesChanged -= OnVisualLinesChanged;
        if (newTextView != null)
            newTextView.VisualLinesChanged += OnVisualLinesChanged;

        base.OnTextViewChanged(oldTextView, newTextView);
    }

    public void Reload()
    {
        InvalidateVisual();
    }

    private void OnVisualLinesChanged(object? sender, EventArgs eventArgs)
    {
        Reload();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(16, 0);
    }

    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(SkEditorAPI.Core.GetApplicationResource("EditorBackgroundColor") as IBrush, null, Bounds);

        var lineHeight = File.Editor.FontSize;
        var lineSpacing = lineHeight * 0.345;
        var scrollViewer = TextEditorEventHandler.GetScrollViewer(File.Editor);
        var scale = File.Editor.FontSize / 12;
        
        for (var line = 1; line <= File.Editor.LineCount; line++)
        {
            var y = (lineSpacing + (line - 1) * lineHeight + (line - 1) * lineSpacing - 1) - scrollViewer.Offset.Y;
            var args = new DrawingArgs(context, File, (float) scale, line, (int) y);
            foreach (var icon in Registries.MarginIcons)
            {
                if (icon.DrawingFunc(args))
                    RenderedIcons[line] = icon;
                else 
                    RenderedIcons.Remove(line);
            }
        }

        base.Render(context);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        var viewer = TextEditorEventHandler.GetScrollViewer(File.Editor);
        var line = (int) (position.Y / (File.Editor.FontSize + File.Editor.FontSize * 0.345) + viewer.Offset.Y / File.Editor.FontSize) + 1;
        
        if (RenderedIcons.ContainsKey(line))
        {
            Cursor = new Cursor(StandardCursorType.Hand);
            HoveredIcon = RenderedIcons[line];
        }
        else
        {
            Cursor = new Cursor(StandardCursorType.Arrow);
            HoveredIcon = null;
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
        HoveredIcon = null;
    }

    protected override async void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (HoveredIcon == null)
            return;
        
        var position = e.GetPosition(this);
        var viewer = TextEditorEventHandler.GetScrollViewer(File.Editor);
        var line = (int) (position.Y / (File.Editor.FontSize + File.Editor.FontSize * 0.345) + viewer.Offset.Y / File.Editor.FontSize) + 1;
        
        var args = new ClickedArgs(File, line);
        HoveredIcon.MouseClickFunc(args);
    }
    
}