using FluentAvalonia.UI.Controls;
using FluentIcons.Avalonia.Fluent;
using SkEditor.Utilities.Icons;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.Models.WhatsNew;

public record WhatsNewFeaturedItem(FAIconSource Icon, string Title, string Description)
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