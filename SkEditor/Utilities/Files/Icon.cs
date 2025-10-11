using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;

namespace SkEditor.Utilities.Files;

public class Icon
{
    public static readonly Dictionary<string, string?> IconDictionary = new()
    {
        { ".sk", "SkriptIcon" }
    };

    public static void SetIcon(OpenedFile openedFile)
    {
        if (openedFile.Path is null)
        {
            return;
        }

        IconSource? iconSource = null;

        string extension = Path.GetExtension(openedFile.Path);
        string? iconName = IconDictionary.GetValueOrDefault(extension);

        if (iconName is not null)
        {
            object? icon = null;
            Application.Current?.TryGetResource(iconName, ThemeVariant.Default, out icon);
            iconSource = icon as PathIconSource;
        }

        if (openedFile.TabViewItem != null)
        {
            openedFile.TabViewItem.IconSource = iconSource;
        }
    }

    public static IconSource? GetIcon(string extension)
    {
        string? iconName = IconDictionary.GetValueOrDefault(extension);

        if (iconName is null)
        {
            return null;
        }

        object? icon = null;
        Application.Current?.TryGetResource(iconName, ThemeVariant.Default, out icon);
        return icon as PathIconSource;
    }
}