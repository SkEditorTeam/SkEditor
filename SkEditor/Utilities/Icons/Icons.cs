using Avalonia;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;

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
}