namespace SkEditor.Core.Dialogs.Common;

public record ElementMargin(double Top, double Bottom, double Left, double Right)
{
    public static ElementMargin Uniform(double spacing)
    {
        return new ElementMargin(spacing, spacing, spacing, spacing);
    }
    
    public static ElementMargin Vertical(double spacing)
    {
        return new ElementMargin(spacing, spacing, 0, 0);
    }
    
    public static ElementMargin Horizontal(double spacing)
    {
        return new ElementMargin(0, 0, spacing, spacing);
    }
}