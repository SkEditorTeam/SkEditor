using Avalonia;

namespace SkEditor.AttachedProperties;

public class BindableStyleClasses
{
    public static readonly AttachedProperty<string> ClassesProperty =
        AvaloniaProperty.RegisterAttached<BindableStyleClasses, StyledElement, string>("Classes", "");

    static BindableStyleClasses() => ClassesProperty.Changed.AddClassHandler<StyledElement>(ClassedAChanged);

    public static void SetClasses(StyledElement obj, string value)
    {
        obj.SetValue(ClassesProperty, value);
    }

    public static string GetClasses(StyledElement obj)
    {
        return obj.GetValue(ClassesProperty);
    }

    private static void ClassedAChanged(StyledElement element, AvaloniaPropertyChangedEventArgs e)
    {
        string newValue = e.NewValue as string ?? string.Empty;

        element.Classes.Clear();
        element.Classes.AddRange(newValue.Split(' '));
    }
}