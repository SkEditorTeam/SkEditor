using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace SkEditor.Converters.Documentation;

public class DocumentationBadgeVisibilityConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 4 ||
            values[0] is not double parentWidth ||
            double.IsNaN(parentWidth) ||
            parentWidth <= 0)
        {
            return false;
        }

        string titleText = values[1] as string ?? "";
        string addonText = values[2] as string ?? "";
        string versionText = values[3] as string ?? "";

        string mode = parameter as string ?? "Version";

        double titleWidth = titleText.Length * 12;
        const double staticWidth = 60;
        const double singleBadgeOverhead = 80;

        if (mode == "Addon")
        {
            if (string.IsNullOrEmpty(addonText))
            {
                return false;
            }

            double addonWidth = (addonText.Length * 8) + singleBadgeOverhead;
            return titleWidth + addonWidth + staticWidth < parentWidth;
        }

        if (string.IsNullOrEmpty(versionText))
        {
            return false;
        }

        double versionWidth = (versionText.Length * 8) + singleBadgeOverhead;

        if (!string.IsNullOrEmpty(addonText))
        {
            double addonWidth = (addonText.Length * 8) + singleBadgeOverhead;
            double totalRequiredWidth = titleWidth + addonWidth + versionWidth + 8 + staticWidth;
            return totalRequiredWidth < parentWidth;
        }

        return titleWidth + versionWidth + staticWidth < parentWidth;
    }
}