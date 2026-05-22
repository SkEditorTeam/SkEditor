using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace SkEditor.Behaviors;

public class FocusOnLoadedBehavior : Behavior<Control>
{
    public static readonly StyledProperty<bool> ShouldFocusProperty =
        AvaloniaProperty.Register<FocusOnLoadedBehavior, bool>(nameof(ShouldFocus));

    public bool ShouldFocus
    {
        get => GetValue(ShouldFocusProperty);
        set => SetValue(ShouldFocusProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject!.Loaded += OnLoaded;
    }

    protected override void OnDetaching()
    {
        AssociatedObject!.Loaded -= OnLoaded;
        base.OnDetaching();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (ShouldFocus)
        {
            AssociatedObject!.Focus();
        }
    }
}