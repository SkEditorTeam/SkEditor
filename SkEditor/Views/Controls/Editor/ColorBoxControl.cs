using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using AvaloniaEdit.Rendering;

namespace SkEditor.Views.Controls.Editor;

public class ColorBoxControl : Control
{
    private readonly Color _color;
    private readonly TextRunProperties _properties;
    private readonly TextView _textView;

    public ColorBoxControl(Color color, TextView textView, TextRunProperties properties)
    {
        _color = color;
        _textView = textView;
        _properties = properties;

        this.Cursor = new Cursor(StandardCursorType.Hand);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double height = _textView.DefaultLineHeight;
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

        context.DrawRectangle(new SolidColorBrush(_color), new Pen(Brushes.Gray, 0.5), rect, borderRadius,
            borderRadius);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = true;

        // todo
    }
}