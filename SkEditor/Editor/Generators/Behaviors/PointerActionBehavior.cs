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

    static PointerActionBehavior() =>
        CommandProperty.Changed.AddClassHandler<Control>((control, e) =>
        {
            control.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);

            if (e.NewValue is ICommand)
            {
                control.AddHandler(
                    InputElement.PointerPressedEvent,
                    OnPointerPressed,
                    RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
                    true);
            }
        });

    public static void SetCommand(Control element, ICommand? value)
    {
        element.SetValue(CommandProperty, value);
    }

    public static ICommand? GetCommand(Control element)
    {
        return element.GetValue(CommandProperty);
    }

    private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control { DataContext: SlotViewModel slot } control)
        {
            return;
        }

        ICommand? command = GetCommand(control);
        if (command is null)
        {
            return;
        }

        PointerUpdateKind kind = e.GetCurrentPoint(control).Properties.PointerUpdateKind;

        ClickType? clickType = kind switch
        {
            PointerUpdateKind.LeftButtonPressed => ClickType.Left,
            PointerUpdateKind.MiddleButtonPressed => ClickType.Middle,
            PointerUpdateKind.RightButtonPressed => ClickType.Right,
            _ => null
        };

        if (clickType is null)
        {
            return;
        }

        SlotPointerAction action = new(slot, clickType.Value);

        if (!command.CanExecute(action))
        {
            return;
        }

        command.Execute(action);
    }
}