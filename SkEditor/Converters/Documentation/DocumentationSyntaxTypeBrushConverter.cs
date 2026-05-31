using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using SkEditor.Models.Documentation;

namespace SkEditor.Converters.Documentation;

public class DocumentationSyntaxTypeBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DocumentationSyntaxType type)
        {
            return new ImmutableSolidColorBrush(Color.Parse("#FFC1C1C1"));
        }

        Color color = type switch
        {
            DocumentationSyntaxType.Event => Color.Parse("#FFFFE07F"),
            DocumentationSyntaxType.Effect => Color.Parse("#FFC99EFF"),
            DocumentationSyntaxType.Expression => Color.Parse("#FF9DFFA7"),
            DocumentationSyntaxType.Condition => Color.Parse("#FFFF5C86"),
            DocumentationSyntaxType.Type => Color.Parse("#FFAFDDFF"),
            DocumentationSyntaxType.Structure => Color.Parse("#FFB3F691"),
            DocumentationSyntaxType.Section => Color.Parse("#FFFF8A73"),
            DocumentationSyntaxType.Function => Color.Parse("#FF44CFC2"),
            _ => Color.Parse("#FFC1C1C1")
        };

        return new ImmutableSolidColorBrush(color);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}