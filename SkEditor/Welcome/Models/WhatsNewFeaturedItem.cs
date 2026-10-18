using FluentAvalonia.UI.Controls;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using SkEditor.Appearance.Icons;

namespace SkEditor.Welcome.Models;

public record WhatsNewFeaturedItem(FAIconSource Icon, string Title, string Description)
{
    public static WhatsNewFeaturedItem Create(Symbol icon, string title, string description)
    {
        return new WhatsNewFeaturedItem(new SymbolIconSource { Symbol = icon }, title, description);
    }

    public static WhatsNewFeaturedItem Create(string pathIconName, string title, string description)
    {
        return new WhatsNewFeaturedItem(Icons.GetPathIconSource(pathIconName), title, description);
    }
}