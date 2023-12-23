using Avalonia;
using FluentAvalonia.UI.Controls;
using System.Collections.Generic;
using System.IO;

namespace SkEditor.Utilities.Files;
public class Icon
{
    public static Dictionary<string, string> IconDictionary = new()
    {
        { ".sk", "SkriptIcon" },
    };

    public static void SetIcon(TabViewItem tabViewItem)
    {
        string tag = tabViewItem.Tag.ToString();

        IconSource iconSource = null;

        string extension = Path.GetExtension(tag);
        string iconName = IconDictionary.GetValueOrDefault(extension);

        if (iconName is not null)
        {
            Application.Current.TryGetResource(iconName, Avalonia.Styling.ThemeVariant.Default, out object icon);
            iconSource = icon as PathIconSource;
        }

        tabViewItem.IconSource = iconSource;
    }
}
