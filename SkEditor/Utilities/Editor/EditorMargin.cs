using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using SkEditor.API;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Parser;

namespace SkEditor.Utilities.Editor;

/// <summary>
///     Represent the left margin column of the code editor. This margin is used to display
///     icons on the left of the editor, next to the line numbers.
///     Can be accessed from any <see cref="OpenedFile" /> as <code>Margin</code> custom data key (
///     <see cref="OpenedFile.CustomData" />)
/// </summary>
public class EditorMargin : AbstractMargin
{
    public EditorMargin(OpenedFile file)
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
        File = file;

        file.Editor.TextArea.LeftMargins.Insert(0, this);
        file.Editor.TextChanged += (_, _) => Reload();
        Reload();
    }

    public Dictionary<(int, int), MarginIconData> RenderedIcons { get; } = new();
    public MarginIconData? HoveredIcon { get; set; }

    public OpenedFile File { get; }

    protected override void OnTextViewChanged(TextView? oldTextView, TextView? newTextView)
    {
        if (oldTextView != null)
        {
            oldTextView.VisualLinesChanged -= OnVisualLinesChanged;
        }

        if (newTextView != null)
        {
            newTextView.VisualLinesChanged += OnVisualLinesChanged;
        }

        base.OnTextViewChanged(oldTextView, newTextView);
    }

    public void Reload()
    {
        InvalidateVisual();
        InvalidateMeasure();
    }

    private void OnVisualLinesChanged(object? sender, EventArgs eventArgs)
    {
        Reload();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double scale = File.Editor.FontSize / 12;

        double GetWidthSelector(MarginIconData icon)
        {
            return icon.GetWidth(scale);
        }

        string GroupByKeySelector(MarginIconData icon)
        {
            return icon.ColumnKey;
        }

        double totalWidth = Registries.MarginIcons
            .GroupBy(GroupByKeySelector)
            .Sum(group => group.Max(GetWidthSelector));

        return new Size(totalWidth, 0);
    }


    public override void Render(DrawingContext context)
    {
        context.DrawRectangle(SkEditorAPI.Core.GetApplicationResource("EditorBackgroundColor") as IBrush, null, Bounds);

        List<int> hidden = FoldingCreator.GetHiddenLines(File);
        double lineHeight = File.Editor.FontSize;
        double lineSpacing = lineHeight * 0.345;
        ScrollViewer scrollViewer = TextEditorEventHandler.GetScrollViewer(File.Editor);
        double scale = File.Editor.FontSize / 12;

        for (int line = 1; line <= File.Editor.LineCount; line++)
        {
            if (hidden.Contains(line))
            {
                continue;
            }

            int separator = hidden.Count(h => h < line);
            double y = lineSpacing + ((line - 1) * lineHeight) + ((line - 1) * lineSpacing) - 1 - scrollViewer.Offset.Y
                       - (separator * (lineSpacing + lineHeight));
            foreach (MarginIconData icon in Registries.MarginIcons)
            {
                (int line, int) key = (line,
                    icon.ColumnKey == null
                        ? 0
                        : Registries.MarginIcons.Select(i => i.ColumnKey).Distinct().ToList().IndexOf(icon.ColumnKey));
                double x = key.Item2 * 16 * scale;

                if (icon.DrawingFunc(new DrawingArgs(context, File, (float)scale, line, (int)y, (int)x)))
                {
                    RenderedIcons[key] = icon;
                }
                else
                {
                    RenderedIcons.Remove(key);
                }
            }
        }

        base.Render(context);
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        Point position = e.GetPosition(this);
        TextViewPosition? point = File.Editor.GetPositionFromPoint(position);
        if (point == null)
        {
            return;
        }

        int line = point.Value.Line;
        int x = (int)position.X / 16;

        foreach (KeyValuePair<(int, int), MarginIconData> icon in RenderedIcons)
        {
            int l = icon.Key.Item1;
            int c = icon.Key.Item2;
            if (line != l || x != c)
            {
                continue;
            }

            Cursor = new Cursor(StandardCursorType.Hand);
            HoveredIcon = icon.Value;
            return;
        }

        Cursor = new Cursor(StandardCursorType.Arrow);
        HoveredIcon = null;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        Cursor = new Cursor(StandardCursorType.Arrow);
        HoveredIcon = null;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (HoveredIcon == null)
        {
            return;
        }

        Point position = e.GetPosition(this);
        TextViewPosition? point = File.Editor.GetPositionFromPoint(position);
        if (point == null)
        {
            return;
        }

        int line = point.Value.Line;

        ClickedArgs args = new(File, line);
        HoveredIcon.MouseClickFunc(args);
    }
}