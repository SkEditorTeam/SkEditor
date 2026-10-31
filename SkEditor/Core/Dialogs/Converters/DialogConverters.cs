using Avalonia.Data.Converters;
using SkEditor.Core.Dialogs.Common;

namespace SkEditor.Core.Dialogs.Converters;

public static class DialogConverters
{
    public static readonly IValueConverter ToHorizontalAlignment =
        new FuncValueConverter<ElementAlignment, Avalonia.Layout.HorizontalAlignment>(alignment =>
        {
            return alignment switch
            {
                ElementAlignment.Left => Avalonia.Layout.HorizontalAlignment.Left,
                ElementAlignment.Center => Avalonia.Layout.HorizontalAlignment.Center,
                ElementAlignment.Right => Avalonia.Layout.HorizontalAlignment.Right,
                _ => Avalonia.Layout.HorizontalAlignment.Left
            };
        });

    public static readonly IValueConverter ToFontWeight =
        new FuncValueConverter<bool, Avalonia.Media.FontWeight>(isBold =>
            isBold ? Avalonia.Media.FontWeight.DemiBold : Avalonia.Media.FontWeight.Normal);

    public static readonly IValueConverter ToFontStyle =
        new FuncValueConverter<bool, Avalonia.Media.FontStyle>(isItalic =>
            isItalic ? Avalonia.Media.FontStyle.Italic : Avalonia.Media.FontStyle.Normal);
    
    public static readonly IValueConverter ToThickness =
        new FuncValueConverter<ElementMargin, Avalonia.Thickness>(spacing => spacing is null ? new Avalonia.Thickness() :
            new Avalonia.Thickness(spacing.Left, spacing.Top, spacing.Right, spacing.Bottom));
}