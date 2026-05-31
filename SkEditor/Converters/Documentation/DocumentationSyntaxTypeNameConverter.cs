using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SkEditor.Models.Documentation;

namespace SkEditor.Converters.Documentation;

public class DocumentationSyntaxTypeNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DocumentationSyntaxType type)
        {
            return string.Empty;
        }

        return type switch
        {
            DocumentationSyntaxType.All => "All",
            DocumentationSyntaxType.Event => "Event",
            DocumentationSyntaxType.Effect => "Effect",
            DocumentationSyntaxType.Expression => "Expression",
            DocumentationSyntaxType.Condition => "Condition",
            DocumentationSyntaxType.Type => "Type",
            DocumentationSyntaxType.Structure => "Structure",
            DocumentationSyntaxType.Section => "Section",
            DocumentationSyntaxType.Function => "Function",
            _ => type.ToString()
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}