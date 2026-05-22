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
using SkEditor.Utilities.Settings;
using SkEditor.Views.Controls.Editor;

namespace SkEditor.Features;

public partial class ColorPreviewFeature(ExperimentsConfig experiments) : IEditorFeature
{
    private readonly HexColorElementGenerator _colorGenerator = new(ShowColorFlyout);

    public bool CanAttach(FileTab tab)
    {
        return experiments.ColorPreviewEnabled && tab.GetExtension() is null or ".sk";
    }

    public void OnViewAttached(FileTab tab, TextEditor editor)
    {
        editor.TextArea.TextView.ElementGenerators.Add(_colorGenerator);
    }

    public void OnViewDetached(FileTab tab, TextEditor editor)
    {
        editor.TextArea.TextView.ElementGenerators.Remove(_colorGenerator);
    }

    private static void ShowColorFlyout(ColorBoxControl control)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(control)!;

        Point topLeft = control.TranslatePoint(new Point(0, 0), topLevel) ?? new Point();

        Flyout flyout = new()
        {
            Placement = PlacementMode.Custom,
            CustomPopupPlacementCallback = placement =>
            {
                const double offset = 8;

                placement.AnchorRectangle = new Rect(
                    topLeft.X,
                    topLeft.Y - offset,
                    control.Bounds.Width,
                    control.Bounds.Height + (offset * 2));

                placement.Anchor = PopupAnchor.BottomLeft;
                placement.Gravity = PopupGravity.BottomRight;
                placement.ConstraintAdjustment =
                    PopupPositionerConstraintAdjustment.FlipY |
                    PopupPositionerConstraintAdjustment.FlipX;
            }
        };

        LayoutTransformControl transformControl = new()
        {
            LayoutTransform = new ScaleTransform(0.9, 0.9)
        };

        ColorView colorPicker = new()
        {
            IsColorPaletteVisible = false,
            IsAlphaEnabled = false,
            IsAlphaVisible = false,
            Color = control.Color,
            Cursor = new Cursor(StandardCursorType.Arrow)
        };

        transformControl.Child = colorPicker;

        int capturedOffset = control.DocumentOffset;
        int capturedLength = control.DocumentLength;
        TextDocument? document = control.TextView.Document;

        document.UndoStack.StartUndoGroup();

        colorPicker.ColorChanged += (_, e) =>
        {
            Color newColor = e.NewColor;
            string newHex = control.IsPrefixed
                ? $"<color:#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}>"
                : $"<#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}>";

            document.Replace(capturedOffset, capturedLength, newHex);
            capturedLength = newHex.Length;
        };

        flyout.Content = transformControl;
        flyout.ShowAt(control.TextView);

        flyout.Closed += (_, _) => document.UndoStack.EndUndoGroup();
    }

    [GeneratedRegex(@"<#[#]?(?<hex>(?:[0-9a-fA-F]{3}){1,2})>")]
    public static partial Regex HexRegexGen();
}