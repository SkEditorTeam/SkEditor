using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Appearance.Icons;
using SkEditor.Workspace.Filesystem.Abstractions;
using SkEditor.Workspace.Filesystem.Sftp;

namespace SkEditor.Workspace.UI.Tree.Converters;

public class NodeToColorConverter : IMultiValueConverter, IValueConverter
{
    private readonly IconProvider _iconProvider = App.Services.GetRequiredService<IconProvider>();

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 1 || values[0] is not FileSystemNode node)
        {
            return Brushes.White;
        }

        return Convert(node, targetType, parameter, culture);
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        Color color = value switch
        {
            SftpFolderNode { IsRoot: true } => GetColorForId("sftp-root"),
            BaseFolderNode => GetColorForId("folder"),
            BaseFileNode file => GetColorForExtension(Path.GetExtension(file.Name)),
            _ => Colors.White
        };

        return new ImmutableSolidColorBrush(color);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private Color GetColorForId(string id)
    {
        string colorString = _iconProvider.GetColorForId(id);
        return Color.TryParse(colorString, out Color parsedColor)
            ? parsedColor
            : Colors.White;
    }

    private Color GetColorForExtension(string extension)
    {
        IconProvider.FileIconEntry iconData = _iconProvider.GetIconForExtension(extension);
        return Color.TryParse(iconData.Color, out Color parsedColor)
            ? parsedColor
            : Colors.White;
    }
}