using Avalonia.Media;
using SkEditor.Utilities.Files;
using System;

namespace SkEditor.API;

/// <summary>
/// Represent an icon attached to a line, displayed in the left margin section of
/// the code editor. This icon is drawn using a provided drawing function.
///
/// All icons with the same column key will be displayed in the same column. If the column key is null,
/// the icon will be displayed in the first column with a random order.
/// </summary>
public record MarginIconData
{
    public Func<DrawingArgs, bool> DrawingFunc { get; }
    public Action<ClickedArgs> MouseClickFunc { get; }
    public string? ColumnKey { get; }
    private readonly Func<double, double>? _widthCalculator;

    /// <summary>
    /// Creates a new MarginIconData instance.
    /// </summary>
    /// <param name="drawingFunc">Function taking DrawingArgs and returning true if something has been drawn, false otherwise.</param>
    /// <param name="mouseClickFunc">Function taking DrawingArgs and called when the icon is clicked.</param>
    /// <param name="columnKey">The column key to use for this icon. If null, the icon will be displayed in the first column.</param>
    /// <param name="widthCalculator">Function that takes a scale factor and returns the required width at that scale. If null, uses default 16 * scale.</param>
    public MarginIconData(
        Func<DrawingArgs, bool> drawingFunc,
        Action<ClickedArgs> mouseClickFunc,
        string? columnKey = null,
        Func<double, double>? widthCalculator = null)
    {
        DrawingFunc = drawingFunc;
        MouseClickFunc = mouseClickFunc;
        ColumnKey = columnKey;
        _widthCalculator = widthCalculator;
    }

    /// <summary>
    /// Gets the required width for this icon at the given scale.
    /// </summary>
    /// <param name="scale">The current scale factor (fontSize / 12)</param>
    public double GetWidth(double scale) => _widthCalculator?.Invoke(scale) ?? 16 * scale;
}

/// <summary>
/// Represent the arguments passed to the drawing function of a <see cref="MarginIconData"/>.
/// </summary>
/// <param name="context">The drawing context to use.</param>
/// <param name="scale">The scale that should be used for drawing, according to the editor's font size. This will be 1 if the font size is 12.</param>
/// <param name="file">The file this icon is currently drawing in.</param>
/// <param name="line">The line this icon is currently drawing at.</param>
/// <param name="y">The starting Y position, representing the top of the line.</param>
public class DrawingArgs(DrawingContext context, OpenedFile file, float scale, int line, int y, int x)
{
    public DrawingContext Context { get; } = context;
    public float Scale { get; } = scale;
    public OpenedFile File { get; } = file;
    public int Line { get; } = line;
    public int Y { get; } = y;
    public int X { get; } = x;
}

/// <summary>
/// Represent the arguments passed to the click function of a <see cref="MarginIconData"/>.
/// </summary>
/// <param name="file">The file this icon has been clicked in.</param>
/// <param name="line">The line this icon has been clicked at.</param>
public class ClickedArgs(OpenedFile file, int line)
{
    public OpenedFile File { get; } = file;
    public int Line { get; } = line;
}