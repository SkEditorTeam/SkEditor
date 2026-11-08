using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SkEditor.Editor.Generators.Models;
using SkEditor.Editor.Generators.ViewModels;

namespace SkEditor.Editor.Generators.Behaviors;

public static class PointerActionBehavior
{
    public static readonly AttachedProperty<ICommand?> CommandProperty =
        AvaloniaProperty.RegisterAttached<Control, ICommand?>("Command", typeof(PointerActionBehavior));

    public static void SetCommand(Control element, ICommand? value) => element.SetValue(CommandProperty, value);
    public static ICommand? GetCommand(Control element) => element.GetValue(CommandProperty);

    static PointerActionBehavior()
    {
        CommandProperty.Changed.AddClassHandler<Control>((control, e) =>
        {
            control.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);

            if (e.NewValue is ICommand)
            {
                control.AddHandler(
                    InputElement.PointerPressedEvent,
                    OnPointerPressed,
                    RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
                    handledEventsToo: true);
            }
        });
    }

    private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control { DataContext: SlotViewModel slot } control)
        {
            return;
        }

        ICommand? command = GetCommand(control);
        if (command is null) return;

        PointerUpdateKind kind = e.GetCurrentPoint(control).Properties.PointerUpdateKind;
        var action = new SlotPointerAction(slot, kind);

        if (!command.CanExecute(action))
        {
            return;
        }

        command.Execute(action);
    }
}