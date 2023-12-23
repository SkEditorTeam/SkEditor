using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit;

namespace SkEditor.Controls;
public class CompletionFlyout : Flyout
{
    public void UpdatePosition()
    {
        if (Popup.HorizontalOffset == HorizontalOffset && Popup.VerticalOffset == VerticalOffset) return;

        Popup.VerticalOffset = VerticalOffset;
        Popup.HorizontalOffset = HorizontalOffset;
    }

    public void ShowAndEdit(TextEditor texteditor)
    {
        ShowAt(texteditor);
        if (Popup.Child is not FlyoutPresenter flyoutPresenter) return;
        flyoutPresenter.Padding = new Thickness(0);
    }
}
