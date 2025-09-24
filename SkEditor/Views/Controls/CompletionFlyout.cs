using System;
using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit;

namespace SkEditor.Views.Controls;

public class CompletionFlyout : Flyout
{
    public void UpdatePosition()
    {
        if (Math.Abs(Popup.HorizontalOffset - HorizontalOffset) < 0.001 &&
            Math.Abs(Popup.VerticalOffset - VerticalOffset) < 0.001)
        {
            return;
        }

        Popup.VerticalOffset = VerticalOffset;
        Popup.HorizontalOffset = HorizontalOffset;
    }

    public void ShowAndEdit(TextEditor texteditor)
    {
        ShowAt(texteditor);
        if (Popup.Child is not FlyoutPresenter flyoutPresenter)
        {
            return;
        }

        flyoutPresenter.Padding = new Thickness(0);
    }
}