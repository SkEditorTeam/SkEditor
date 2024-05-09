using Avalonia;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Controls;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.API;
using SkEditor.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;
using SkEditor.Utilities.Parser;

namespace SkEditor.Utilities.Editor;
public class TooltipHandler
{
    private static Flyout Flyout { get; } = new()
    {
        PlacementAnchor = PopupAnchor.BottomRight,
        Placement = PlacementMode.Pointer,
        ShowMode = FlyoutShowMode.TransientWithDismissOnPointerMoveAway,
        HorizontalOffset = -2.5,
        VerticalOffset = -2.5,
    };

    public static void OnPointerHover(object? sender, PointerEventArgs e)
    {
        TextEditor editor = ApiVault.Get().GetTextEditor();
        if (editor == null) return;

        Point pos = e.GetPosition(editor.TextArea.TextView);

        int line = TextEditorUtilities.GetLineNumberFromMousePosition(pos, editor.TextArea);
        if (line == -1) return;

        SimpleSegment segment = TextEditorUtilities.GetWordAtMousePosition(pos, editor.TextArea);
        if (segment == SimpleSegment.Invalid) return;

        if (SkDocParser.GetFunction(editor, line) is null 
            && !SkDocParser.IsFunctionCall(editor, segment)) return;

        FunctionTooltip tooltip = new();
        Flyout.Content = tooltip;

        void pointerMoved(object? _, PointerEventArgs e2)
        {
            var position = e2.GetPosition(editor);
            var delta = new Vector(e.GetPosition(editor).X - position.X,
                e.GetPosition(editor).Y - position.Y);

            if (!(Math.Abs(delta.X) > 30) && !(Math.Abs(delta.Y) > 30)) return;
            Flyout.Hide();
            ApiVault.Get().GetMainWindow().PointerMoved -= pointerMoved;
        }

        ApiVault.Get().GetMainWindow().PointerMoved += pointerMoved;

        FlyoutBase.SetAttachedFlyout(editor, Flyout);
        Flyout.ShowAt(editor);
    }
}