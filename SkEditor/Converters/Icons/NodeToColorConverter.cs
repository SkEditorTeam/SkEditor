using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Models.Projects.Sftp;
using SkEditor.Models.Projects.Types;
using SkEditor.Services;

namespace SkEditor.Converters.Icons;

public class NodeToColorConverter : IValueConverter
{
    private readonly IconProvider _iconProvider = App.Services.GetRequiredService<IconProvider>();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        switch (value)
        {
            case SftpFolderNode { IsRoot: true }:
                return new ImmutableSolidColorBrush(Color.Parse("#8CA9FF"));
            case BaseFolderNode:
                return new ImmutableSolidColorBrush(Color.Parse("#FFCF40"));
            case BaseFileNode file:
            {
                string ext = Path.GetExtension(file.Name);
                IconProvider.FileIconEntry iconData = _iconProvider.GetIconForExtension(ext);

                return new ImmutableSolidColorBrush(Color.Parse(iconData.Color));
            }
            default:
                return Brushes.White;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}