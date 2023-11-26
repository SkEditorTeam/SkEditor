using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using ExCSS;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using System.Collections.Generic;
using System.Linq;

namespace SkEditor.Views;
public partial class FontSelectionWindow : AppWindow
{
	private static FontInfo? _selectedFont;

	public FontSelectionWindow()
	{
		InitializeComponent();

		SetUp();
		LoadFonts();
	}

	private void SetUp()
	{
		SelectButton.Command = new RelayCommand(() =>
		{
			string fontName = _selectedFont?.Name;
			Close(fontName);
		});

		SearchBox.TextChanged += (s, e) =>
		{
			List<string> fonts = FontListBox.Items.Cast<FontInfo>().Select(x => x.Name).ToList();
			FontListBox.SelectedItem = FontListBox.Items
				.Cast<FontInfo>()
				.FirstOrDefault(x => x.Name.ToLower().StartsWith(SearchBox.Text.ToLower()));
		};

		FontListBox.SelectionChanged += (s, e) =>
		{
			_selectedFont = FontListBox.SelectedItem as FontInfo;
		};
	}

	private void LoadFonts()
	{
		List<FontInfo> fonts = FontManager.Current.SystemFonts
			.Select(font => new FontInfo { Name = font.Name, Font = font })
			.ToList();

		FontListBox.ItemsSource = fonts;

		FontListBox.SelectedItem = ApiVault.Get().GetAppConfig().Font;
		FontListBox.ScrollIntoView(FontListBox.SelectedItem);
	}
}
public class FontInfo
{
	public string Name { get; set; }
	public FontFamily Font { get; set; }
}
