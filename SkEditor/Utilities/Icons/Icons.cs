using Avalonia;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.Utilities.Icons;

public static class Icons
{
    public static FAPathIconSource GetPathIcon(string name)
    {
        object? icon = null;
        Application.Current?.TryGetResource(name, ThemeVariant.Default, out icon);
        FAPathIconSource? iconSource = icon as FAPathIconSource;
        return iconSource ?? new FAPathIconSource();
    }

    public static SymbolIconSource CreateSymbolIcon(Symbol symbol)
    {
        return new SymbolIconSource
        {
            Symbol = symbol
        };
    }
}