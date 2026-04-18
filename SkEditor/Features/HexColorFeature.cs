using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.ElementGenerators;
using SkEditor.Models.Tabs;
using SkEditor.Transformers;
using SkEditor.Utilities.Settings;
using SkEditor.Views.Controls.Editor;

namespace SkEditor.Features;

public partial class HexColorFeature(ExperimentsConfig experiments) : IEditorFeature
{
    private readonly HexColorElementGenerator _colorGenerator = new(ShowColorFlyout);
    private readonly HexColorTransformer _colorTransformer = new();

    public bool CanAttach(FileTab tab)
    {
        return experiments.HexPreviewEnabled;
    }

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        editor.TextArea.TextView.ElementGenerators.Add(_colorGenerator);
        editor.TextArea.TextView.LineTransformers.Add(_colorTransformer);
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        editor.TextArea.TextView.ElementGenerators.Remove(_colorGenerator);
        editor.TextArea.TextView.LineTransformers.Remove(_colorTransformer);
    }

    private static void ShowColorFlyout(ColorBoxControl control)
    {
        Point pos = control.TranslatePoint(
            new Point(0, control.Bounds.Height), control.TextView) ?? new Point();

        Flyout flyout = new()
        {
            Placement = PlacementMode.AnchorAndGravity,
            PlacementAnchor = PopupAnchor.TopLeft,
            PlacementGravity = PopupGravity.BottomRight,
            PlacementConstraintAdjustment = PopupPositionerConstraintAdjustment.All,
            HorizontalOffset = pos.X,
            VerticalOffset = pos.Y + 8
        };
        ColorView colorPicker = new()
        {
            IsColorPaletteVisible = false,
            IsAlphaEnabled = false,
            IsAlphaVisible = false,
            Color = control.Color,
            Cursor = new Cursor(StandardCursorType.Arrow)
        };

        int capturedOffset = control.DocumentOffset;
        int capturedLength = control.DocumentLength;
        TextDocument? document = control.TextView.Document;

        document.UndoStack.StartUndoGroup();

        colorPicker.ColorChanged += (_, e) =>
        {
            Color newColor = e.NewColor;
            string newHex = $"<#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}>";

            document.Replace(capturedOffset, capturedLength, newHex);
            capturedLength = newHex.Length;
        };

        flyout.Content = colorPicker;
        flyout.ShowAt(control.TextView);

        flyout.Closed += (_, _) => document.UndoStack.EndUndoGroup();
    }

    [GeneratedRegex(@"<#[#]?(?<hex>(?:[0-9a-fA-F]{3}){1,2})>")]
    public static partial Regex HexRegexGen();
}