using Avalonia;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;
using Symbol = FluentIcons.Common.Symbol;
using SkEditor.API;

namespace SkEditor.Utilities.Icons;

public static class Icons
{
    public static PathIconSource GetPathIcon(string name)
    {
        object? icon = null;
        Application.Current?.TryGetResource(name, ThemeVariant.Default, out icon);
        PathIconSource? iconSource = icon as PathIconSource;
        return iconSource ?? new PathIconSource();
    }
    
    public static SymbolIconSource CreateSymbolIcon(Symbol symbol)
    {
        return new SymbolIconSource
        {
            Symbol = symbol
        };
    }

    public static IconSource? CreatePathIconSource(string name)
    {
        return SkEditorAPI.Core.GetApplicationResource(name) as PathIconSource;
    }
}