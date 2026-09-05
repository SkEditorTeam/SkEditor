using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using SkEditor.Models.Completions;

namespace SkEditor.Converters.Completions;

public class CompletionIconColorConverter : IValueConverter
{
    public static readonly CompletionIconColorConverter Instance = new();

    private static readonly ImmutableSolidColorBrush EffectBrush = new(Color.Parse("#FFC99EFF"));
    private static readonly ImmutableSolidColorBrush KeywordBrush = new(Color.Parse("#FFB3F691"));
    private static readonly ImmutableSolidColorBrush SectionBrush = new(Color.Parse("#FFFF8A73"));
    private static readonly ImmutableSolidColorBrush FunctionBrush = new(Color.Parse("#FF44CFC2"));
    private static readonly ImmutableSolidColorBrush VariableBrush = new(Color.Parse("#FFAFDDFF"));
    private static readonly ImmutableSolidColorBrush SnippetBrush = new(Color.Parse("#FFFFE07F"));
    private static readonly ImmutableSolidColorBrush DefaultBrush = new(Color.Parse("#FFC1C1C1"));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CompletionType type)
        {
            return DefaultBrush;
        }

        return type switch
        {
            CompletionType.Effect => EffectBrush,
            CompletionType.Keyword => KeywordBrush,
            CompletionType.Section => SectionBrush,
            CompletionType.Function => FunctionBrush,
            CompletionType.Variable => VariableBrush,
            CompletionType.Snippet => SnippetBrush,
            _ => DefaultBrush
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}