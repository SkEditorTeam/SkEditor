using Avalonia;
using FluentAvalonia.UI.Controls;
using System.IO;

namespace SkEditor.Utilities.Files;
public class IconSetter
{
	public static void SetIcon(TabViewItem tabViewItem)
	{
		string tag = tabViewItem.Tag.ToString();

		IconSource iconSource = null;

		string iconName = GetIconName(Path.GetExtension(tag));

		if (iconName is not null)
		{
			Application.Current.TryGetResource(iconName, Avalonia.Styling.ThemeVariant.Default, out object icon);
			iconSource = icon as PathIconSource;
		}

		tabViewItem.IconSource = iconSource;
	}

	private static string GetIconName(string extension) => extension switch
	{
		".sk" => "SkriptIcon",
		_ => null,
	};
}
