using System;
using System.Globalization;
using Avalonia.Data.Converters;
using SkEditor.Core.Localization;
using SkEditor.Documentation.Models;

namespace SkEditor.Documentation.UI.Converters;

public class DocumentationSyntaxTypeNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DocumentationSyntaxType type)
        {
            return string.Empty;
        }

        TranslationKey key = type switch
        {
            DocumentationSyntaxType.All => TranslationKey.DocumentationSyntaxTypeAll,
            DocumentationSyntaxType.Event => TranslationKey.DocumentationSyntaxTypeEvent,
            DocumentationSyntaxType.Effect => TranslationKey.DocumentationSyntaxTypeEffect,
            DocumentationSyntaxType.Expression => TranslationKey.DocumentationSyntaxTypeExpression,
            DocumentationSyntaxType.Condition => TranslationKey.DocumentationSyntaxTypeCondition,
            DocumentationSyntaxType.Type => TranslationKey.DocumentationSyntaxTypeType,
            DocumentationSyntaxType.Structure => TranslationKey.DocumentationSyntaxTypeStructure,
            DocumentationSyntaxType.Section => TranslationKey.DocumentationSyntaxTypeSection,
            DocumentationSyntaxType.Function => TranslationKey.DocumentationSyntaxTypeFunction,
            _ => TranslationKey.None
        };

        return key;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}