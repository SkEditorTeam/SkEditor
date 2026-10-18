using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace SkEditor.Layout.StatusBar.UI;

public class SkEditorTabStrip : TabStrip
{
    protected override Type StyleKeyOverride => typeof(SkEditorTabStrip);

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new SkEditorTabStripItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<SkEditorTabStripItem>(item, out recycleKey);
    }
}

public class SkEditorTabStripItem : TabStripItem
{
    private bool _isPressed;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isPressed = true;
            Focus();
            e.Handled = true;
        }

        base.OnPointerPressed(e);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (_isPressed && e.InitialPressMouseButton == MouseButton.Left)
        {
            _isPressed = false;

            PointerPoint point = e.GetCurrentPoint(this);
            Rect bounds = new(Bounds.Size);

            if (bounds.Contains(point.Position))
            {
                SelectingItemsControl? parent = SelectingItemsControl.ItemsControlFromItemContainer(this);
                if (parent != null)
                {
                    int index = parent.IndexFromContainer(this);
                    if (index != -1)
                    {
                        parent.SelectedIndex = index;
                    }
                }

                e.Handled = true;
            }
        }

        base.OnPointerReleased(e);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        _isPressed = false;
        base.OnPointerCaptureLost(e);
    }
}