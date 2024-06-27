using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.Controls;
using SkEditor.Utilities.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkEditor.Utilities.Completion;
public static partial class CompletionHandler
{
    public static CompletionFlyout CompletionPopup { get; } = new()
    {
        PlacementAnchor = PopupAnchor.BottomRight,
        Placement = PlacementMode.BottomEdgeAlignedRight,
        ShowMode = FlyoutShowMode.Transient,
    };

    private static TextEditor _currentTextEditor;

    public async static void OnTextChanged(object sender, EventArgs e)
    {
        TextEditor textEditor = (TextEditor)sender;
        _currentTextEditor = textEditor;
        TextDocument document = textEditor.Document;

        var segment = TextEditorUtilities.GetSegmentBeforeOffset(textEditor.TextArea.Caret.Offset, document);
        if (segment == SimpleSegment.Invalid)
        {
            CompletionPopup.Hide();
            return;
        }

        var word = document.GetText(segment.Offset, segment.Length);

        IEnumerable<CompletionItem> completions = CompletionProvider.GetCompletions(word, textEditor);
        if (!completions.Any())
        {
            CompletionPopup.Hide();
            return;
        }
        CompletionMenu completionMenu;

        if (CompletionPopup.IsOpen)
        {

            completionMenu = (CompletionMenu)CompletionPopup.Content;
            completionMenu.CompletionListBox.ItemsSource = null;
            completionMenu.CompletionListBox.Items.Clear();
            completionMenu.SetItems(completions);
        }
        else
        {
            completionMenu = new(CompletionProvider.GetCompletions(word, textEditor));
        }

        CompletionPopup.Content = completionMenu;

        var caret = textEditor.TextArea.Caret.CalculateCaretRectangle();
        var pointOnScreen = textEditor.TextArea.TextView.PointToScreen(caret.Position - textEditor.TextArea.TextView.ScrollOffset);
        CompletionPopup.HorizontalOffset = pointOnScreen.X + 10;
        CompletionPopup.VerticalOffset = pointOnScreen.Y + 25;

        if (CompletionPopup.IsOpen)
        {
            CompletionPopup.UpdatePosition();
        }
        else
        {
            CompletionPopup.ShowAndEdit(textEditor);
        }

        await Dispatcher.UIThread.InvokeAsync(() => completionMenu.CompletionListBox.SelectedIndex = 0);
    }

    public static void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (!CompletionPopup.IsOpen)
        {
            return;
        }

        switch (e.Key)
        {
            case Key.Escape:
                e.Handled = true;
                CompletionPopup.Hide();
                break;

            case Key.Up:
                e.Handled = true;
                HandleArrowKey(true);
                break;
            case Key.Down:
                e.Handled = true;
                HandleArrowKey(false);
                break;

            case Key.Enter:
            case Key.Tab:
                e.Handled = true;
                CompletionMenu completionMenu = (CompletionMenu)CompletionPopup.Content;
                var listBox = completionMenu.CompletionListBox;
                var selectedItem = (ListBoxItem)listBox.SelectedItem;
                var completionItem = (CompletionItem)selectedItem.Tag;
                CompletionPopup.Hide();
                OnCompletion(completionItem);
                break;
        }
    }

    private static void HandleArrowKey(bool isUpKey)
    {
        var completionMenu = (CompletionMenu)CompletionPopup.Content;
        var listBox = completionMenu.CompletionListBox;

        int selectedIndex = listBox.SelectedIndex;
        int itemCount = listBox.Items.Count;

        listBox.SelectedIndex = isUpKey ? (selectedIndex == 0 ? itemCount - 1 : selectedIndex - 1)
                                      : (selectedIndex == itemCount - 1 ? 0 : selectedIndex + 1);
    }

    private static void OnCompletion(CompletionItem completionItem)
    {
        
        var caretLineOffset = _currentTextEditor.Document.GetLineByNumber(_currentTextEditor.TextArea.Caret.Line).Offset;
        while (char.IsControl(Convert.ToChar(_currentTextEditor.Document.GetText(caretLineOffset, 1))))
            caretLineOffset++;
        
        //_currentTextEditor.CaretOffset = caretLineOffset;
        _currentTextEditor.Document.Remove(caretLineOffset, _currentTextEditor.TextArea.Caret.Offset - caretLineOffset);
        
        var snippet = CompletionParser.Parse(completionItem.Content);
        snippet.Insert(_currentTextEditor.TextArea);
    }

    [GeneratedRegex("\n(?!\\t)")]
    private static partial Regex NewLineWithoutTabRegex();
}