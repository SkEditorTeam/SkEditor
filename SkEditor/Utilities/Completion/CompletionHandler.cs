using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using SkEditor.Controls;
using SkEditor.Utilities.Editor;

namespace SkEditor.Utilities.Completion;

public static partial class CompletionHandler
{
    private static TextEditor? _currentTextEditor;

    public static CompletionFlyout CompletionPopup { get; } = new()
    {
        PlacementAnchor = PopupAnchor.BottomRight,
        Placement = PlacementMode.BottomEdgeAlignedRight,
        ShowMode = FlyoutShowMode.Transient
    };

    public static async void OnTextChanged(object? sender, EventArgs e)
    {
        TextEditor? textEditor = (TextEditor?)sender;
        if (textEditor == null) return;
        
        _currentTextEditor = textEditor;
        TextDocument document = textEditor.Document;

        SimpleSegment segment = TextEditorUtilities.GetSegmentBeforeOffset(textEditor.TextArea.Caret.Offset, document);
        if (segment == SimpleSegment.Invalid)
        {
            CompletionPopup.Hide();
            return;
        }

        string? word = document.GetText(segment.Offset, segment.Length);

        IEnumerable<CompletionItem> completions = CompletionProvider.GetCompletions(word, textEditor);
        IEnumerable<CompletionItem> completionItems = completions as CompletionItem[] ?? completions.ToArray();
        if (!completionItems.Any())
        {
            CompletionPopup.Hide();
            return;
        }

        CompletionMenu? completionMenu;

        if (CompletionPopup.IsOpen)
        {
            completionMenu = (CompletionMenu?)CompletionPopup.Content;
            if (completionMenu == null)
            {
                completionMenu = new CompletionMenu(completionItems);
            }
            else
            {
                completionMenu.CompletionListBox.ItemsSource = null;
                completionMenu.CompletionListBox.Items.Clear();
                completionMenu.SetItems(completionItems);
            }
        }
        else
        {
            completionMenu = new CompletionMenu(CompletionProvider.GetCompletions(word, textEditor));
        }

        CompletionPopup.Content = completionMenu;

        Rect caret = textEditor.TextArea.Caret.CalculateCaretRectangle();
        PixelPoint pointOnScreen =
            textEditor.TextArea.TextView.PointToScreen(caret.Position - textEditor.TextArea.TextView.ScrollOffset);
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

    public static void OnKeyDown(object? sender, KeyEventArgs e)
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
                CompletionMenu? completionMenu = (CompletionMenu?)CompletionPopup.Content;
                ListBox? listBox = completionMenu?.CompletionListBox;
                ListBoxItem? selectedItem = (ListBoxItem?)listBox?.SelectedItem;
                CompletionItem? completionItem = (CompletionItem?)selectedItem?.Tag;
                CompletionPopup.Hide();
                if (completionItem is not null) OnCompletion(completionItem);
                break;
        }
    }

    private static void HandleArrowKey(bool isUpKey)
    {
        CompletionMenu? completionMenu = (CompletionMenu?)CompletionPopup.Content;
        if (completionMenu == null) return;
        ListBox? listBox = completionMenu.CompletionListBox;

        int selectedIndex = listBox.SelectedIndex;
        int itemCount = listBox.Items.Count;

        listBox.SelectedIndex = isUpKey ? selectedIndex == 0 ? itemCount - 1 : selectedIndex - 1
            : selectedIndex == itemCount - 1 ? 0 : selectedIndex + 1;
    }

    private static void OnCompletion(CompletionItem completionItem)
    {
        if (_currentTextEditor == null) return;
        
        int offset = _currentTextEditor.TextArea.Caret.Offset;
        SimpleSegment segment = TextEditorUtilities.GetSegmentBeforeOffset(offset, _currentTextEditor.Document);
        if (segment == SimpleSegment.Invalid)
        {
            return;
        }

        string content = completionItem.Content;
        if (content.Contains('\t'))
        {
            DocumentLine? line = _currentTextEditor.Document.GetLineByOffset(segment.Offset);
            string? lineText = _currentTextEditor.Document.GetText(line.Offset, line.Length);
            int tabs = lineText.Count(c => c == '\t');

            content = string.Join("",
                content.Split('\t').Select((part, index) => index > 0 ? new string('\t', tabs + 1) + part : part));
        }

        if (content.Contains('\n'))
        {
            DocumentLine? line = _currentTextEditor.Document.GetLineByOffset(segment.Offset);
            string? lineText = _currentTextEditor.Document.GetText(line.Offset, line.Length);
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