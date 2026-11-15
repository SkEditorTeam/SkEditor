using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace SkEditor.Appearance.Behaviors;

public class TextBoxMoveCaretToEndOnFocusBehavior : StyledElementBehavior<TextBox>
{
    protected override void OnAttachedToVisualTree()
    {
        AssociatedObject?.AddHandler(InputElement.GotFocusEvent, AssociatedObject_GotFocus, RoutingStrategies.Bubble);
    }

    protected override void OnDetachedFromVisualTree()
    {
        AssociatedObject?.RemoveHandler(InputElement.GotFocusEvent, AssociatedObject_GotFocus);
    }

    private void AssociatedObject_GotFocus(object? sender, FocusChangedEventArgs e)
    {
        AssociatedObject?.CaretIndex = AssociatedObject.Text?.Length ?? 0;
    }
}