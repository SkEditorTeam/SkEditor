using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Icons;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Models.WhatsNew;

public record WhatsNewFeaturedItem(IconSource Icon, string Title, string Description)
{
    public static WhatsNewFeaturedItem Create(Symbol icon, string title, string description)
    {
        return new WhatsNewFeaturedItem(new SymbolIconSource { Symbol = icon }, title, description);
    }

    public static WhatsNewFeaturedItem Create(string pathIconName, string title, string description)
    {
        return new WhatsNewFeaturedItem(Icons.GetPathIcon(pathIconName), title, description);
    }
}