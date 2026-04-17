using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit;
using SkEditor.ElementGenerators;
using SkEditor.Models.Tabs;
using SkEditor.Transformers;
using SkEditor.Utilities.Settings;

namespace SkEditor.Features;

public partial class HexColorFeature : IEditorFeature
{
    private readonly HexColorElementGenerator _colorGenerator;
    private readonly HexColorTransformer _colorTransformer;
    private readonly ExperimentsConfig _experiments;

    public HexColorFeature(ExperimentsConfig experiments)
    {
        _experiments = experiments;

        _colorTransformer = new HexColorTransformer();

        _colorGenerator = new HexColorElementGenerator(control =>
        {
            Point pos = control.TranslatePoint(
                new Point(0, control.Bounds.Height), control.TextView) ?? new Point();

            Flyout flyout = new()
            {
                Placement = PlacementMode.AnchorAndGravity,
                PlacementAnchor = PopupAnchor.TopLeft,
                PlacementGravity = PopupGravity.BottomRight,
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
            var document = control.TextView.Document;

            colorPicker.ColorChanged += (s, e) =>
            {
                Color newColor = e.NewColor;
                string newHex = $"<#{newColor.R:X2}{newColor.G:X2}{newColor.B:X2}>";

                document.BeginUpdate();
                try
                {
                    document.Replace(capturedOffset, capturedLength, newHex);
                    capturedLength = newHex.Length;
                }
                finally
                {
                    document.EndUpdate();
                }
            };

            flyout.Content = colorPicker;
            flyout.ShowAt(control.TextView);
        });
    }

    public bool CanAttach(FileTab tab)
    {
        return _experiments.HexPreviewEnabled;
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

    [GeneratedRegex(@"<#[#]?(?<hex>(?:[0-9a-fA-F]{3}){1,2})>")]
    public static partial Regex HexRegexGen();
}