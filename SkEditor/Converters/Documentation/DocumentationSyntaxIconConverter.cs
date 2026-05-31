using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using SkEditor.Models.Documentation;

namespace SkEditor.Converters.Documentation;

public class DocumentationSyntaxIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DocumentationSyntaxType type)
        {
            return new SymbolIconSource { Symbol = Symbol.QuestionCircle };
        }

        Symbol symbol = type switch
        {
            DocumentationSyntaxType.Event => Symbol.Flash,
            DocumentationSyntaxType.Effect => Symbol.Wand,
            DocumentationSyntaxType.Expression => Symbol.BracesVariable,
            DocumentationSyntaxType.Condition => Symbol.ShieldQuestion,
            DocumentationSyntaxType.Type => Symbol.Tag,
            DocumentationSyntaxType.Structure => Symbol.Organization,
            DocumentationSyntaxType.Section => Symbol.AlignLeft,
            DocumentationSyntaxType.Function => Symbol.MathFormula,
            _ => Symbol.QuestionCircle
        };

        return new SymbolIconSource { Symbol = symbol };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}