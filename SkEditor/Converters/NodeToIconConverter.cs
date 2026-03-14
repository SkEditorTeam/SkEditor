using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using FluentIcons.Common;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Models.Projects;
using SkEditor.Models.Projects.Sftp;
using SkEditor.Services;

namespace SkEditor.Converters;

public class NodeToIconConverter : IMultiValueConverter, IValueConverter
{
    private readonly IconProvider _iconProvider = App.Services.GetRequiredService<IconProvider>();

    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2 || values[0] is not FileSystemNode node || values[1] is not bool)
        {
            return _iconProvider.DefaultIcon;
        }

        return Convert(node, targetType, parameter, culture);
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        switch (value)
        {
            case SftpFolderNode folder:
                return folder.IsRoot
                    ? Symbol.Desktop
                    : folder is { IsExpanded: true, HasChildren: true }
                        ? Symbol.FolderOpen
                        : Symbol.Folder;
            case BaseFolderNode baseFolder:
                return baseFolder is { IsExpanded: true, HasChildren: true }
                    ? Symbol.FolderOpen
                    : Symbol.Folder;
        }

        if (value is not BaseFileNode file)
        {
            return _iconProvider.DefaultIcon;
        }

        string ext = Path.GetExtension(file.Name);
        IconProvider.FileIconEntry iconData = _iconProvider.GetIconForExtension(ext);

        return Enum.TryParse(iconData.Icon, out Symbol symbol)
            ? symbol
            : _iconProvider.DefaultIcon;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}