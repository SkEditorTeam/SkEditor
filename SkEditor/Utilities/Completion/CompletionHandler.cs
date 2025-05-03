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
        IEnumerable<CompletionItem> completionItems = completions as CompletionItem[] ?? completions.ToArray();
        if (!completionItems.Any())
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
            completionMenu.SetItems(completionItems);
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
        int offset = _currentTextEditor.TextArea.Caret.Offset;
        var segment = TextEditorUtilities.GetSegmentBeforeOffset(offset, _currentTextEditor.Document);
        if (segment == SimpleSegment.Invalid)
        {
            return;
        }

        string content = completionItem.Content;
        if (content.Contains('\t'))
        {
            var line = _currentTextEditor.Document.GetLineByOffset(segment.Offset);
            var lineText = _currentTextEditor.Document.GetText(line.Offset, line.Length);
            int tabs = lineText.Count(c => c == '\t');

            content = string.Join("", content.Split('\t').Select((part, index) => index > 0 ? new string('\t', tabs + 1) + part : part));
        }

        if (content.Contains('\n'))
        {
            var line = _currentTextEditor.Document.GetLineByOffset(segment.Offset);
            var lineText = _currentTextEditor.Document.GetText(line.Offset, line.Length);
            int tabs = lineText.Count(c => c == '\t');

            content = NewLineWithoutTabRegex().Replace(content, "\n" + new string('\t', tabs));
        }


        int caretOffset = completionItem.Content.IndexOf("{c}", StringComparison.Ordinal);
        if (caretOffset != -1)
        {
            content = content.Remove(caretOffset, 3);
            _currentTextEditor.Document.Replace(segment.Offset, segment.Length, content);
            _currentTextEditor.TextArea.Caret.Offset = segment.Offset + caretOffset;
        }
        else
        {
            _currentTextEditor.Document.Replace(segment.Offset, segment.Length, completionItem.Content);
            _currentTextEditor.TextArea.Caret.Offset = segment.Offset + completionItem.Content.Length;
        }
    }

    [GeneratedRegex("\n(?!\\t)")]
    private static partial Regex NewLineWithoutTabRegex();
}