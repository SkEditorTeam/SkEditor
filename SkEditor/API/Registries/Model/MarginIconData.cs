using System;
using Avalonia.Media;
using SkEditor.Utilities.Files;

namespace SkEditor.API;

/// <summary>
/// Represent an icon attached to a line, displayed in the left margin section of
/// the code editor. This icon is drawn using a provided drawing function.
/// </summary>
/// <param name="DrawingFunc">Function taking <see cref="DrawingArgs"/> and returning true if something has been drawn, false otherwise.</param>
/// <param name="MouseClickFunc">Function taking <see cref="DrawingArgs"/> and called when the icon is clicked.</param>
public record MarginIconData(Func<DrawingArgs, bool> DrawingFunc, 
    Action<ClickedArgs> MouseClickFunc);

/// <summary>
/// Represent the arguments passed to the drawing function of a <see cref="MarginIconData"/>.
/// </summary>
/// <param name="context">The drawing context to use.</param>
/// <param name="scale">The scale that should be used for drawing, according to the editor's font size. This will be 1 if the font size is 12.</param>
/// <param name="file">The file this icon is currently drawing in.</param>
/// <param name="line">The line this icon is currently drawing at.</param>
/// <param name="y">The starting Y position, representing the top of the line.</param>
public class DrawingArgs(DrawingContext context, OpenedFile file, float scale, int line, int y)
{
    public DrawingContext Context { get; } = context;
    public float Scale { get; } = scale;
    public OpenedFile File { get; } = file;
    public int Line { get; } = line;
    public int Y { get; } = y;
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