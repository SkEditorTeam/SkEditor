using Avalonia.Data.Converters;

namespace SkEditor.Editor.Generators.Converters;

public static class ItemConverters
{
    public static readonly IValueConverter IsGreaterThanOne =
        new FuncValueConverter<int, bool>(amount => amount > 1);
}