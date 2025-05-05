using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;

namespace SkEditor.Utilities.Files;

public class Icon
{
    public static Dictionary<string, string> IconDictionary = new()
    {
        { ".sk", "SkriptIcon" }
    };

    public static void SetIcon(OpenedFile openedFile)
    {
        if (openedFile.Path is null)
        {
            return;
        }

        IconSource iconSource = null;

        string extension = Path.GetExtension(openedFile.Path);
        string iconName = IconDictionary.GetValueOrDefault(extension);

        if (iconName is not null)
        {
            Application.Current.TryGetResource(iconName, ThemeVariant.Default, out object icon);
            iconSource = icon as PathIconSource;
        }

        openedFile.TabViewItem.IconSource = iconSource;
    }

    public static IconSource? GetIcon(string extension)
    {
        string iconName = IconDictionary.GetValueOrDefault(extension);

        if (iconName is not null)
        {
            Application.Current.TryGetResource(iconName, ThemeVariant.Default, out object icon);
            return icon as PathIconSource;
        }

        return null;
    }
}