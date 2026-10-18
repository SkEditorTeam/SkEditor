using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SkEditor.Workspace.Tabs.Converters;

public class TabIconConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        object? icon = values[0];
        object? isLoading = values[1];

        return isLoading is true ? null : icon;
    }
}