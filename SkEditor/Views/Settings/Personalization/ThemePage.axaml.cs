using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Controls;
using SkEditor.Utilities.Styling;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SkEditor.Views.Settings;
public partial class ThemePage : UserControl
{
	public static Dictionary<ColorPickerSettingsExpander, string> colorMappings = new();

	public ThemePage()
	{
		InitializeComponent();

		AssignCommands();

		LoadThemes();
	}

	private void LoadThemes()
	{
		foreach (Theme theme in ThemeEditor.Themes)
		{
			ComboBoxItem item = new()
			{
				Content = theme.Name,
				Tag = theme.FileName
			};

			ThemeComboBox.Items.Add(item);
		}

		ThemeComboBox.SelectedItem = ThemeComboBox.Items.FirstOrDefault(x => ((ComboBoxItem)x).Tag.Equals(ThemeEditor.CurrentTheme.FileName));

		ThemeComboBox.SelectionChanged += (s, e) =>
		{
			ComboBoxItem item = (ComboBoxItem)ThemeComboBox.SelectedItem;
			Theme theme = ThemeEditor.Themes.FirstOrDefault(x => x.FileName.Equals(item.Tag));
			ThemeEditor.SetTheme(theme);
		};
	}


	private void AssignCommands()
	{
		Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(PersonalizationPage)));

		OpenThemesFolderButton.Command = new RelayCommand(() => Process.Start(new ProcessStartInfo()
		{
			FileName = ThemeEditor.ThemeFolderPath,
			UseShellExecute = true
		}));
	}
}
