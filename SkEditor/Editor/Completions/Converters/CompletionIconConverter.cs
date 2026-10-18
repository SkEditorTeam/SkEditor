using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentIcons.Common;
using SkEditor.Models.Completions;

namespace SkEditor.Converters.Completions;

public class CompletionIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CompletionType type)
        {
            return Symbol.QuestionCircle;
        }

        Symbol symbol = type switch
        {
            CompletionType.Effect => Symbol.Wand,
            CompletionType.Keyword => Symbol.Tag,
            CompletionType.Section => Symbol.AlignLeft,
            CompletionType.Function => Symbol.MathFormula,
            CompletionType.Variable => Symbol.BracesVariable,
            CompletionType.Snippet => Symbol.PuzzlePiece,
            _ => Symbol.QuestionCircle
        };

        return symbol;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}