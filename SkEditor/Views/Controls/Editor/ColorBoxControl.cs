using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using AvaloniaEdit.Rendering;

namespace SkEditor.Views.Controls.Editor;

public class ColorBoxControl : Control
{
    private readonly Action<ColorBoxControl> _clickAction;

    private readonly TextRunProperties _properties;

    public ColorBoxControl(Color color, TextView textView, TextRunProperties properties,
        Action<ColorBoxControl> clickAction,
        int documentOffset,
        int documentLength)
    {
        Color = color;
        TextView = textView;
        _properties = properties;
        _clickAction = clickAction;
        DocumentOffset = documentOffset;
        DocumentLength = documentLength;

        this.Cursor = new Cursor(StandardCursorType.Hand);
    }

    public Color Color { get; }
    public int DocumentOffset { get; }
    public int DocumentLength { get; }
    public TextView TextView { get; }

    protected override Size MeasureOverride(Size availableSize)
    {
        double height = TextView.DefaultLineHeight;
        double boxSize = height * 0.7;

        return new Size(boxSize, _properties.FontRenderingEmSize);
    }

    public override void Render(DrawingContext context)
    {
        double height = Bounds.Height;
        double boxSize = height * 0.75;
        double yOffset = (height - boxSize) / 2;

        var rect = new Rect(0, yOffset, boxSize, boxSize);

        double borderRadius = boxSize * 0.1;

        context.DrawRectangle(new SolidColorBrush(Color), new Pen(Brushes.Gray, 0.5), rect, borderRadius,
            borderRadius);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = true;

        _clickAction(this);
    }
}