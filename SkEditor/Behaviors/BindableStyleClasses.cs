using Avalonia;

namespace SkEditor.Behaviors;

public class BindableStyleClasses
{
    static BindableStyleClasses() => ClassesProperty.Changed.AddClassHandler<StyledElement>(ClassedAChanged);

    private static void ClassedAChanged(StyledElement element, AvaloniaPropertyChangedEventArgs e)
    {
        string newValue = e.NewValue as string ?? string.Empty;

        element.Classes.Clear();
        element.Classes.AddRange(newValue.Split(' '));
    }


    public static readonly AttachedProperty<string> ClassesProperty =
        AvaloniaProperty.RegisterAttached<BindableStyleClasses, StyledElement, string>("Classes", "");

    public static void SetClasses(StyledElement obj, string value)
    {
        obj.SetValue(ClassesProperty, value);
    }

    public static string GetClasses(StyledElement obj)
    {
        return obj.GetValue(ClassesProperty);
    }
}